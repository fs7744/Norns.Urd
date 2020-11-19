using System.Reflection;

namespace Norns.Urd.Reflection
{
    public static class PropertyExtensions
    {
        public static PropertyReflector GetReflector(this PropertyInfo property)
        {
            static PropertyReflector Create(PropertyInfo property) => new PropertyReflector(property);
            return ReflectorCache<PropertyInfo, PropertyReflector>.GetOrAdd(property, Create);
        }
    }
}