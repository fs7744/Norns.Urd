using System;
using System.Collections.Generic;
using System.Reflection;

namespace Norns.Urd.Reflection
{
    public class CustomAttributeReflector
    {
        public CustomAttributeReflector(CustomAttributeData data)
        {
            Data = data;
            Tokens = GetAttrTokens(data.AttributeType);
        }

        public CustomAttributeData Data { get; }

        public HashSet<RuntimeTypeHandle> Tokens { get; }

        private static HashSet<RuntimeTypeHandle> GetAttrTokens(Type attributeType)
        {
            var tokenSet = new HashSet<RuntimeTypeHandle>();
            for (var attr = attributeType; attr != typeof(object); attr = attr.GetTypeInfo().BaseType)
            {
                tokenSet.Add(attr.TypeHandle);
            }
            return tokenSet;
        }

        internal static CustomAttributeReflector Create(CustomAttributeData customAttributeData)
        {
            return ReflectorCache<CustomAttributeData, CustomAttributeReflector>.GetOrAdd(customAttributeData, data => new CustomAttributeReflector(data));
        }
    }
}