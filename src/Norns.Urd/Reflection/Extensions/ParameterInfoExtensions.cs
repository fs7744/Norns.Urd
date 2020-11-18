using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;

namespace Norns.Urd.Reflection
{
    public static class ParameterInfoExtensions
    {
        public static bool IsReadOnly(this ParameterInfo parameter)
        {
            // C# `in` parameters are also by-ref, but meant to be read-only.
            // The section "Metadata representation of in parameters" on the following page
            // defines how such parameters are marked:
            //
            // https://github.com/dotnet/csharplang/blob/master/proposals/csharp-7.2/readonly-ref.md
            //
            // This poses three problems for detecting them:
            //
            //  * The C# Roslyn compiler marks `in` parameters with an `[in]` IL modifier,
            //    but this isn't specified, nor is it used uniquely for `in` params.
            //
            //  * `System.Runtime.CompilerServices.IsReadOnlyAttribute` is not defined on all
            //    .NET platforms, so the compiler sometimes recreates that type in the same
            //    assembly that contains the method having an `in` parameter. In other words,
            //    it's an attribute one must check for by name (which is slow, as it implies
            //    use of a `GetCustomAttributes` enumeration instead of a faster `IsDefined`).
            //
            //  * A required custom modifier `System.Runtime.InteropServices.InAttribute`
            //    is always present in those cases relevant for DynamicProxy (proxyable methods),
            //    but not all targeted platforms support reading custom modifiers. Also,
            //    support for cmods is generally flaky (at this time of writing, mid-2018).
            //
            // The above points inform the following detection logic: First, we rely on an IL
            // `[in]` modifier being present. This is a "fast guard" against non-`in` parameters:
            if ((parameter.Attributes & (ParameterAttributes.In | ParameterAttributes.Out)) != ParameterAttributes.In)
            {
                return false;
            }

            // This check allows to make the detection logic more robust on the platforms which support custom modifiers.
            // The robustness is achieved by the fact, that usually the `IsReadOnlyAttribute` emitted by the compiler is internal to the assembly.
            // Therefore, if clients use Reflection.Emit to create "a copy" of the methods with read-only members, they cannot re-use the existing attribute.
            // Instead, they are forced to emit their own `IsReadOnlyAttribute` to mark some argument as immutable.
            // The `InAttribute` type OTOH was always available in BCL. Therefore, it's much easier to copy the modreq and be recognized by Castle.
            //
            // If check fails, resort to the IsReadOnlyAttribute check.
            // Check for the required modifiers first, as it's faster.
            if (parameter.GetRequiredCustomModifiers().Any(x => x == typeof(InAttribute)))
            {
                return true;
            }

            // The comparison by name is intentional; any assembly could define that attribute.
            // See explanation in comment above.
            if (parameter.GetCustomAttributes(false).Any(x => x.GetType().FullName == "System.Runtime.CompilerServices.IsReadOnlyAttribute"))
            {
                return true;
            }

            return false;
        }

        public static bool HasDefaultValueByAttributes(this ParameterInfo parameter)
        {
            // parameter.HasDefaultValue will throw a FormatException when parameter is DateTime type with default value
            return (parameter.Attributes & ParameterAttributes.HasDefault) != 0;
        }

        public static void CopyDefaultValueConstant(this ParameterBuilder to, ParameterInfo from)
        {
            object defaultValue;
            try
            {
                defaultValue = from.DefaultValue;
            }
            catch (FormatException) when (from.ParameterType == typeof(DateTime))
            {
                // This catch clause guards against a CLR bug that makes it impossible to query
                // the default value of an optional DateTime parameter. For the CoreCLR, see
                // https://github.com/dotnet/corefx/issues/26164.

                // If this bug is present, it is caused by a `null` default value:
                defaultValue = null;
            }
            catch (FormatException) when (from.ParameterType.GetTypeInfo().IsEnum)
            {
                // This catch clause guards against a CLR bug that makes it impossible to query
                // the default value of a (closed generic) enum parameter. For the CoreCLR, see
                // https://github.com/dotnet/corefx/issues/29570.

                // If this bug is present, it is caused by a `null` default value:
                defaultValue = null;
            }

            if (defaultValue is Missing)
            {
                // It is likely that we are reflecting over invalid metadata if we end up here.
                // At this point, `to.Attributes` will have the `HasDefault` flag set. If we do
                // not call `to.SetConstant`, that flag will be reset when creating the dynamic
                // type, so `to` will at least end up having valid metadata. It is quite likely
                // that the `Missing.Value` will still be reproduced because the `Parameter-
                // Builder`'s `ParameterAttributes.Optional` is likely set. (If it isn't set,
                // we'll be causing a default value of `DBNull.Value`, but there's nothing that
                // can be done about that, short of recreating a new `ParameterBuilder`.)
                return;
            }

            try
            {
                to.SetConstant(defaultValue);
            }
            catch (ArgumentException)
            {
                var parameterType = from.ParameterType;
                var parameterNonNullableType = parameterType;
                var isNullableType = parameterType.IsNullableType();

                if (defaultValue == null)
                {
                    if (isNullableType)
                    {
                        // This guards against a Mono bug that prohibits setting default value `null`
                        // for a `Nullable<T>` parameter. See https://github.com/mono/mono/issues/8504.
                        //
                        // If this bug is present, luckily we still get `null` as the default value if
                        // we do nothing more (which is probably itself yet another bug, as the CLR
                        // would "produce" a default value of `Missing.Value` in this situation).
                        return;
                    }
                    else if (parameterType.GetTypeInfo().IsValueType)
                    {
                        // This guards against a CLR bug that prohibits replicating `null` default
                        // values for non-nullable value types (which, despite the apparent type
                        // mismatch, is perfectly legal and something that the Roslyn compilers do).
                        // For the CoreCLR, see https://github.com/dotnet/corefx/issues/26184.

                        // If this bug is present, the best we can do is to not set the default value.
                        // This will cause a default value of `Missing.Value` (if `ParameterAttributes-
                        // .Optional` is set) or `DBNull.Value` (otherwise, unlikely).
                        return;
                    }
                }
                else if (isNullableType)
                {
                    parameterNonNullableType = from.ParameterType.GetGenericArguments()[0];
                    if (parameterNonNullableType.GetTypeInfo().IsEnum || parameterNonNullableType.IsInstanceOfType(defaultValue))
                    {
                        // This guards against two bugs:
                        //
                        // * On the CLR and CoreCLR, a bug that makes it impossible to use `ParameterBuilder-
                        //   .SetConstant` on parameters of a nullable enum type. For CoreCLR, see
                        //   https://github.com/dotnet/coreclr/issues/17893.
                        //
                        //   If this bug is present, there is no way to faithfully reproduce the default
                        //   value. This will most likely cause a default value of `Missing.Value` or
                        //   `DBNull.Value`. (To better understand which of these, see comment above).
                        //
                        // * On Mono, a bug that performs a too-strict type check for nullable types. The
                        //   value passed to `ParameterBuilder.SetConstant` must have a type matching that
                        //   of the parameter precisely. See https://github.com/mono/mono/issues/8597.
                        //
                        //   If this bug is present, there's no way to reproduce the default value because
                        //   we cannot actually create a value of type `Nullable<>`.
                        return;
                    }
                }

                // Finally, we might have got here because the metadata constant simply doesn't match
                // the parameter type exactly. Some code generators other than the .NET compilers
                // might produce such metadata. Make a final attempt to coerce it to the required type:
                try
                {
                    var coercedDefaultValue = Convert.ChangeType(defaultValue, parameterNonNullableType, CultureInfo.InvariantCulture);
                    to.SetConstant(coercedDefaultValue);

                    return;
                }
                catch
                {
                    // We don't care about the error thrown by an unsuccessful type coercion.
                }

                throw;
            }
        }
    }
}