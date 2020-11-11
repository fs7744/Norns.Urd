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
                name.Append("<");
                name.Append(GetDisplayName(arguments[0].GetTypeInfo()));
                for (var i = 1; i < arguments.Length; i++)
                {
                    name.Append(",");
                    name.Append(GetDisplayName(arguments[i].GetTypeInfo()));
                }
                name.Append(">");
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
                name.Append("<");
                name.Append(GetFullDisplayName(arguments[0].GetTypeInfo()));
                for (var i = 1; i < arguments.Length; i++)
                {
                    name.Append(",");
                    name.Append(GetFullDisplayName(arguments[i].GetTypeInfo()));
                }
                name.Append(">");
            }
            return name.ToString();
        }

        public static string GetProxyTypeName(this TypeInfo serviceType, ProxyTypes proxyType)
        {
            return $"{Constants.GeneratedNamespace}.{serviceType.GetDisplayName()}_Proxy_{proxyType}";
        }

        public static TypeBuilder DefineProxyType(this ModuleBuilder module, TypeInfo serviceType, ProxyTypes proxyType)
        {
            var name = serviceType.GetProxyTypeName(proxyType);
            var pType = serviceType.IsInterface
                ? module.DefineType(name, ProxyTypeAttributes, typeof(object), new Type[] { serviceType })
                : module.DefineType(name, ProxyTypeAttributes, serviceType, Type.EmptyTypes);
            DefineGenericParameter(pType, serviceType);
            return pType;
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
    }
}