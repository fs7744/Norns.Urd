using System.Reflection;
using System.Runtime.CompilerServices;

namespace Norns.Urd.Reflection
{
    public static class PropertyExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static PropertyReflector Create(PropertyInfo property) => new PropertyReflector(property);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static PropertyReflector GetReflector(this PropertyInfo property)
        {
            return ReflectorCache<PropertyInfo, PropertyReflector>.GetOrAdd(property, Create);
        }
    }
}