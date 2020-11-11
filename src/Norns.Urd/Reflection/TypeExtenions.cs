using Norns.Urd.DynamicProxy;
using System.Reflection;
using System.Text;

namespace Norns.Urd.Reflection
{
    public static class TypeExtenions
    {
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

        public static
    }
}