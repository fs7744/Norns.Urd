using System.Reflection;

namespace Norns.Urd.Reflection
{
    public static class PropertyExtensions
    {
        private static PropertyReflector Create(PropertyInfo property) => new PropertyReflector(property);

        public static PropertyReflector GetReflector(this PropertyInfo property)
        {
            return ReflectorCache<PropertyInfo, PropertyReflector>.GetOrAdd(property, Create);
        }
    }
}