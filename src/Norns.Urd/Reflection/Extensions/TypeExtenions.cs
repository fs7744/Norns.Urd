using Norns.Urd.Attributes;
using Norns.Urd.DynamicProxy;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Norns.Urd.Reflection
{
    public static class TypeExtenions
    {
        private const TypeAttributes ProxyTypeAttributes = TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed;

        #region Name

        public static string GetDisplayName(this TypeInfo typeInfo)
        {
            var name = new StringBuilder(typeInfo.Name).Replace('+', '.');
            if (typeInfo.IsGenericParameter)
            {
                return name.ToString();
            }
            if (typeInfo.IsGenericType)
            {
                var arguments = typeInfo.IsGenericTypeDefinition
                 ? typeInfo.GenericTypeParameters
                 : typeInfo.GenericTypeArguments;
                name = name.Replace("`", "").Replace(arguments.Length.ToString(), "");
                name.Append('<');
                name.Append(GetDisplayName(arguments[0].GetTypeInfo()));
                for (var i = 1; i < arguments.Length; i++)
                {
                    name.Append(',');
                    name.Append(GetDisplayName(arguments[i].GetTypeInfo()));
                }
                name.Append('>');
            }
            if (typeInfo.IsNested)
            {
                name.Insert(0, ".");
                name.Insert(0, GetDisplayName(typeInfo.DeclaringType.GetTypeInfo()));
            }
            return name.ToString();
        }

        public static string GetFullDisplayName(this TypeInfo typeInfo)
        {
            var name = new StringBuilder(typeInfo.Name).Replace('+', '.');
            if (typeInfo.IsGenericParameter)
            {
                return name.ToString();
            }
            name.Insert(0, ".");
            if (typeInfo.IsNested)
            {
                name.Insert(0, GetFullDisplayName(typeInfo.DeclaringType.GetTypeInfo()));
            }
            else
            {
                name.Insert(0, typeInfo.Namespace);
            }
            if (typeInfo.IsGenericType)
            {
                var arguments = typeInfo.IsGenericTypeDefinition
                 ? typeInfo.GenericTypeParameters
                 : typeInfo.GenericTypeArguments;
                name = name.Replace("`", "").Replace(arguments.Length.ToString(), "");
                name.Append('<');
                name.Append(GetFullDisplayName(arguments[0].GetTypeInfo()));
                for (var i = 1; i < arguments.Length; i++)
                {
                    name.Append(',');
                    name.Append(GetFullDisplayName(arguments[i].GetTypeInfo()));
                }
                name.Append('>');
            }
            return name.ToString();
        }

        public static string GetProxyTypeName(this TypeInfo serviceType, ProxyTypes proxyType) =>
            $"{Constants.GeneratedNamespace}.{serviceType.GetDisplayName()}_Proxy_{proxyType}";

        #endregion Name

        #region Type

        public static bool AreEquivalent(TypeInfo t1, TypeInfo t2) => t1 == t2
            || t1.IsEquivalentTo(t2.AsType());

        public static Type GetNonNullableType(this TypeInfo type) => type switch
        {
            _ when type.IsNullableType() => type.GetGenericArguments()[0],
            _ => type.AsType()
        };

        public static bool IsNullableType(this Type type) => type.GetTypeInfo().IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(Nullable<>);

        public static bool IsNullableType(this TypeInfo type) => type.IsGenericType
            && type.GetGenericTypeDefinition() == typeof(Nullable<>);

        public static bool IsLegalExplicitVariantDelegateConversion(this TypeInfo source, TypeInfo dest)
        {
            if (!source.IsDelegate() || !dest.IsDelegate() || !source.IsGenericType || !dest.IsGenericType)
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

                if (genericParameter.IsInvariant())
                {
                    return false;
                }

                if (genericParameter.IsCovariant())
                {
                    if (!sourceArgument.HasReferenceConversion(destArgument))
                    {
                        return false;
                    }
                }
                else if (genericParameter.IsContravariant() && (sourceArgument.IsValueType || destArgument.IsValueType))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsDelegate(this TypeInfo t) => t.IsSubclassOf(typeof(MulticastDelegate));

        public static bool IsInvariant(this TypeInfo t) => 0 == (t.GenericParameterAttributes & GenericParameterAttributes.VarianceMask);

        public static bool IsCovariant(this TypeInfo t) => 0 != (t.GenericParameterAttributes & GenericParameterAttributes.Covariant);

        public static bool IsContravariant(this TypeInfo t) => 0 != (t.GenericParameterAttributes & GenericParameterAttributes.Contravariant);

        public static bool HasReferenceConversion(this TypeInfo source, TypeInfo dest)
        {
            static bool CheckNonNullableType(TypeInfo s, TypeInfo d)
            {
                var nnSourceType = s.GetNonNullableType().GetTypeInfo();
                var nnDestType = d.GetNonNullableType().GetTypeInfo();
                return nnSourceType switch
                {
                    _ when nnSourceType.IsAssignableFrom(nnDestType)
                        || nnDestType.IsAssignableFrom(nnSourceType) => true,
                    _ => false
                };
            }

            var sourceTyppe = source.AsType();
            var destTyppe = dest.AsType();
            return source switch
            {
                _ when sourceTyppe == typeof(void) || destTyppe == typeof(void) => false,
                _ when CheckNonNullableType(source, dest)
                    || source.IsInterface
                    || dest.IsInterface
                    || IsLegalExplicitVariantDelegateConversion(source, dest)
                    || sourceTyppe == typeof(object)
                    || destTyppe == typeof(object) => true,
                _ => false,
            };
        }

        public static bool IsConvertible(this TypeInfo typeInfo)
        {
            if (typeInfo.IsEnum)
            {
                return true;
            }
            return Type.GetTypeCode(GetNonNullableType(typeInfo)) switch
            {
                TypeCode.Boolean
                or TypeCode.Byte
                or TypeCode.SByte
                or TypeCode.Int16
                or TypeCode.Int32
                or TypeCode.Int64
                or TypeCode.UInt16
                or TypeCode.UInt32
                or TypeCode.UInt64
                or TypeCode.Single
                or TypeCode.Double
                or TypeCode.Char
                => true,
                _ => false,
            };
        }

        public static bool IsVisible(this TypeInfo typeInfo)
        {
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

        public static bool IsUnsigned(this TypeInfo typeInfo)
        {
            return Type.GetTypeCode(GetNonNullableType(typeInfo)) switch
            {
                TypeCode.Byte
                or TypeCode.UInt16
                or TypeCode.Char
                or TypeCode.UInt32
                or TypeCode.UInt64
                => true,
                _ => false,
            };
        }

        public static bool IsFloatingPoint(this TypeInfo typeInfo)
        {
            return Type.GetTypeCode(GetNonNullableType(typeInfo)) switch
            {
                TypeCode.Single or TypeCode.Double => true,
                _ => false,
            };
        }

        public static MemberReflector<TypeInfo> GetReflector(this TypeInfo type)
        {
            static MemberReflector<TypeInfo> Create(TypeInfo t) => new MemberReflector<TypeInfo>(t);
            return ReflectorCache<TypeInfo, MemberReflector<TypeInfo>>.GetOrAdd(type, Create);
        }

        public static MemberReflector<TypeInfo> GetReflector(this Type type)
        {
            return type.GetTypeInfo().GetReflector();
        }

        public static bool IsProxyType(this TypeInfo type)
        {
            return type.GetReflector().IsDefined<DynamicProxyAttribute>();
        }

        public static bool IsProxyType(this Type type)
        {
            return type.GetTypeInfo().IsProxyType();
        }

        #endregion Type

        #region Emit

        public static TypeBuilder DefineProxyType(this ModuleBuilder module, TypeInfo serviceType, ProxyTypes proxyType)
        {
            var name = serviceType.GetProxyTypeName(proxyType);
            var pType = serviceType.IsInterface
                ? module.DefineType(name, ProxyTypeAttributes, typeof(object), new Type[] { serviceType })
                : module.DefineType(name, ProxyTypeAttributes, serviceType, Type.EmptyTypes);
            DefineGenericParameter(pType, serviceType);
            return pType;
        }

        public static TypeBuilder DefineProxyAssistType(this ModuleBuilder module, TypeBuilder proxyType)
        {
            return module.DefineType($"{proxyType.FullName}_Assist", ProxyTypeAttributes, typeof(object), Type.EmptyTypes);
        }

        private static void DefineGenericParameter(TypeBuilder typeBuilder, Type targetType)
        {
            if (targetType.GetTypeInfo().IsGenericTypeDefinition)
            {
                var genericArguments = targetType.GetTypeInfo().GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
                var genericArgumentsBuilders = typeBuilder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
                for (var index = 0; index < genericArguments.Length; index++)
                {
                    genericArgumentsBuilders[index].SetGenericParameterAttributes(ToClassGenericParameterAttributes(genericArguments[index].GenericParameterAttributes));
                    foreach (var constraint in genericArguments[index].GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                    {
                        if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                        if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                    }
                }
            }
        }

        private static GenericParameterAttributes ToClassGenericParameterAttributes(GenericParameterAttributes attributes)
        {
            if (attributes == GenericParameterAttributes.None)
            {
                return GenericParameterAttributes.None;
            }
            if (attributes.HasFlag(GenericParameterAttributes.SpecialConstraintMask))
            {
                return GenericParameterAttributes.SpecialConstraintMask;
            }
            if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            {
                return GenericParameterAttributes.NotNullableValueTypeConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                return GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                return GenericParameterAttributes.ReferenceTypeConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                return GenericParameterAttributes.DefaultConstructorConstraint;
            }
            return GenericParameterAttributes.None;
        }

        #endregion Emit
    }
}