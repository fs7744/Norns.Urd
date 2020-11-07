using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Threading.Tasks;

namespace Norns.Urd.Extensions
{
    public static class TypeExtensions
    {
        private static readonly ConcurrentDictionary<TypeInfo, bool> isTaskOfTCache = new ConcurrentDictionary<TypeInfo, bool>();
        private static readonly ConcurrentDictionary<TypeInfo, bool> isValueTaskOfTCache = new ConcurrentDictionary<TypeInfo, bool>();

        public static object GetDefaultValue(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            if (typeInfo.AsType() == typeof(void))
            {
                return null;
            }

            switch (Type.GetTypeCode(typeInfo.AsType()))
            {
                case TypeCode.Object:
                case TypeCode.DateTime:
                    if (typeInfo.IsValueType)
                    {
                        return Activator.CreateInstance(typeInfo.AsType());
                    }
                    else
                    {
                        return null;
                    }

                case TypeCode.Empty:
                case TypeCode.String:
                    return null;

                case TypeCode.Boolean:
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                    return 0;

                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return 0;

                case TypeCode.Single:
                    return default(Single);

                case TypeCode.Double:
                    return default(Double);

                case TypeCode.Decimal:
                    return new Decimal(0);

                default:
                    throw new InvalidOperationException("Code supposed to be unreachable.");
            }
        }

        public static object GetDefaultValue(this Type type)
        {
            return type?.GetTypeInfo()?.GetDefaultValue();
        }

        public static bool IsVisible(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            if (typeInfo.IsNested)
            {
                if (!typeInfo.DeclaringType.GetTypeInfo().IsVisible())
                {
                    return false;
                }
                if (!typeInfo.IsVisible || !typeInfo.IsNestedPublic)
                {
                    return false;
                }
            }
            else
            {
                if (!typeInfo.IsVisible || !typeInfo.IsPublic)
                {
                    return false;
                }
            }
            if (typeInfo.IsGenericType && !typeInfo.IsGenericTypeDefinition)
            {
                foreach (var argument in typeInfo.GenericTypeArguments)
                {
                    if (!argument.GetTypeInfo().IsVisible())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public static bool IsTask(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.AsType() == typeof(Task);
        }

        public static bool IsTaskWithResult(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return isTaskOfTCache.GetOrAdd(typeInfo, Info => Info.IsGenericType && typeof(Task).GetTypeInfo().IsAssignableFrom(Info));
        }

        public static bool IsValueTask(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return typeInfo.AsType() == typeof(ValueTask);
        }

        public static bool IsValueTaskWithResult(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            return isValueTaskOfTCache.GetOrAdd(typeInfo, Info => Info.IsGenericType && Info.GetGenericTypeDefinition() == typeof(ValueTask<>));
        }

        public static bool IsNullableType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool AreEquivalent(TypeInfo t1, TypeInfo t2)
        {
            return t1 == t2 || t1.IsEquivalentTo(t2.AsType());
        }

        public static bool IsNullableType(this TypeInfo type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static Type GetNonNullableType(this TypeInfo type)
        {
            if (IsNullableType(type))
            {
                return type.GetGenericArguments()[0];
            }
            return type.AsType();
        }

        public static bool IsLegalExplicitVariantDelegateConversion(TypeInfo source, TypeInfo dest)
        {
            if (!IsDelegate(source) || !IsDelegate(dest) || !source.IsGenericType || !dest.IsGenericType)
                return false;

            var genericDelegate = source.GetGenericTypeDefinition();

            if (dest.GetGenericTypeDefinition() != genericDelegate)
                return false;

            var genericParameters = genericDelegate.GetTypeInfo().GetGenericArguments();
            var sourceArguments = source.GetGenericArguments();
            var destArguments = dest.GetGenericArguments();

            for (int iParam = 0; iParam < genericParameters.Length; ++iParam)
            {
                var sourceArgument = sourceArguments[iParam].GetTypeInfo();
                var destArgument = destArguments[iParam].GetTypeInfo();

                if (AreEquivalent(sourceArgument, destArgument))
                {
                    continue;
                }

                var genericParameter = genericParameters[iParam].GetTypeInfo();

                if (IsInvariant(genericParameter))
                {
                    return false;
                }

                if (IsCovariant(genericParameter))
                {
                    if (!HasReferenceConversion(sourceArgument, destArgument))
                    {
                        return false;
                    }
                }
                else if (IsContravariant(genericParameter) && (sourceArgument.IsValueType || destArgument.IsValueType))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsDelegate(TypeInfo t)
        {
            return t.IsSubclassOf(typeof(System.MulticastDelegate));
        }

        public static bool IsInvariant(TypeInfo t)
        {
            return 0 == (t.GenericParameterAttributes & GenericParameterAttributes.VarianceMask);
        }

        public static bool IsCovariant(this TypeInfo t)
        {
            return 0 != (t.GenericParameterAttributes & GenericParameterAttributes.Covariant);
        }

        public static bool HasReferenceConversion(TypeInfo source, TypeInfo dest)
        {
            // void -> void conversion is handled elsewhere
            // (it's an identity conversion)
            // All other void conversions are disallowed.
            if (source.AsType() == typeof(void) || dest.AsType() == typeof(void))
            {
                return false;
            }

            var nnSourceType = TypeExtensions.GetNonNullableType(source).GetTypeInfo();
            var nnDestType = TypeExtensions.GetNonNullableType(dest).GetTypeInfo();

            // Down conversion
            if (nnSourceType.IsAssignableFrom(nnDestType))
            {
                return true;
            }
            // Up conversion
            if (nnDestType.IsAssignableFrom(nnSourceType))
            {
                return true;
            }
            // Interface conversion
            if (source.IsInterface || dest.IsInterface)
            {
                return true;
            }
            // Variant delegate conversion
            if (IsLegalExplicitVariantDelegateConversion(source, dest))
                return true;

            // Object conversion
            if (source.AsType() == typeof(object) || dest.AsType() == typeof(object))
            {
                return true;
            }
            return false;
        }

        public static bool IsContravariant(TypeInfo t)
        {
            return 0 != (t.GenericParameterAttributes & GenericParameterAttributes.Contravariant);
        }

        public static bool IsConvertible(this TypeInfo typeInfo)
        {
            var type = GetNonNullableType(typeInfo);
            if (typeInfo.IsEnum)
            {
                return true;
            }
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Boolean:
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Char:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsUnsigned(TypeInfo typeInfo)
        {
            var type = GetNonNullableType(typeInfo);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.UInt16:
                case TypeCode.Char:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                    return true;
                default:
                    return false;
            }
        }

        public static bool IsFloatingPoint(TypeInfo typeInfo)
        {
            var type = GetNonNullableType(typeInfo);
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Single:
                case TypeCode.Double:
                    return true;
                default:
                    return false;
            }
        }
    }
}