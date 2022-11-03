using System.Reflection;
using System.Text;

namespace Norns.Urd.Reflection
{
    public class TypeReflector : MemberReflector<TypeInfo>
    {
        public string DisplayName { get; }
        public string FullDisplayName { get; }

        public TypeReflector(TypeInfo type) : base(type)
        {
            DisplayName = GetDisplayName(type);
            FullDisplayName = GetFullDisplayName(type);
        }

        private static string GetDisplayName(TypeInfo typeInfo)
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

        private static string GetFullDisplayName(TypeInfo typeInfo)
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
    }
}