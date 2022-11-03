using System;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Norns.Urd.Reflection
{
    internal static class ReflectorCache<TMemberInfo, TReflector>
    {
        private readonly static ConcurrentDictionary<TMemberInfo, TReflector> cache = new ConcurrentDictionary<TMemberInfo, TReflector>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static TReflector GetOrAdd(TMemberInfo key, Func<TMemberInfo, TReflector> factory)
        {
            return cache.GetOrAdd(key, k => factory(k));
        }
    }
}