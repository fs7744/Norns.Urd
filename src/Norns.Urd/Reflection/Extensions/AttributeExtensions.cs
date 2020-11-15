using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;

namespace Norns.Urd.Reflection
{
    public static class AttributeExtensions
    {
        public static CustomAttributeBuilder DefineCustomAttribute(Type attributeType)
        {
            return new CustomAttributeBuilder(attributeType.GetConstructor(Type.EmptyTypes), Array.Empty<object>());
        }

        public static CustomAttributeBuilder DefineCustomAttribute<T>() where T : Attribute
        {
            return DefineCustomAttribute(typeof(T));
        }

        public static CustomAttributeReflector[] GetCustomAttributeReflectors(this ICustomAttributeReflectorProvider provider)
        {
            return provider.CustomAttributeReflectors;
        }

        public static IEnumerable<CustomAttributeReflector> GetCustomAttributeReflectors(this ICustomAttributeReflectorProvider provider, Type attributeType)
        {
            var reflectors = provider.GetCustomAttributeReflectors();
            if (reflectors.Length == 0)
            {
                return reflectors;
            }
            else
            {
                var attrToken = attributeType.TypeHandle;
                return reflectors.Where(i => i.Tokens.Contains(attrToken));
            }
        }

        public static bool IsDefined(this ICustomAttributeReflectorProvider provider, Type attributeType)
        {
            return provider.GetCustomAttributeReflectors(attributeType).FirstOrDefault() != null;
        }

        public static bool IsDefined<T>(this ICustomAttributeReflectorProvider provider) where T : Attribute
        {
            return provider.IsDefined(typeof(T));
        }
    }
}