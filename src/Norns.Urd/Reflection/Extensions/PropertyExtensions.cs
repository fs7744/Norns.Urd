using System;
using System.Reflection;
using System.Reflection.Emit;
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

        public static Func<T, R> CreateGetter<T, R>(this PropertyInfo property)
        {
            if (!property.CanRead) return null;
            var t = typeof(T);
            var r = typeof(R);
            var method = new DynamicMethod(Guid.NewGuid().ToString("N"), r, new Type[] { t });
            var il = method.GetILGenerator();
            il.EmitLoadArg(0);
            il.Emit(OpCodes.Callvirt, property.GetMethod);
            il.EmitConvertTo(property.PropertyType, r);
            il.Emit(OpCodes.Ret);
            return (Func<T, R>)method.CreateDelegate(typeof(Func<T, R>));
        }
    }
}