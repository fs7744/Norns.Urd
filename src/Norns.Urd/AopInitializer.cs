using Norns.Urd.DynamicProxy;
using Norns.Urd.Interceptors;
using Norns.Urd.Reflection;
using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Norns.Urd
{
    public static class AopInitializer
    {
        private static readonly ConcurrentDictionary<Type, Action<object, object>> instanceSetterCache = new ConcurrentDictionary<Type, Action<object, object>>();

        public static IProxyGenerator Init(this Action<IInterceptorConfiguration> config)
        {
            var configuration = new InterceptorConfiguration();
            config?.Invoke(configuration);
            return new ProxyGenerator(configuration);
        }

        public static Func<object, object> CreateInstanceGetter(this Type type)
        {
            var field = type.GetField(Constants.Instance, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.CreateGetter();
        }

        public static Action<object, object> CreateInstanceSetter(this Type type)
        {
            return instanceSetterCache.GetOrAdd(type, InternalCreateInstanceSetter);
        }

        private static Action<object, object> InternalCreateInstanceSetter(this Type type)
        {
            var field = type.GetField(Constants.Instance, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.CreateSetter();
        }
    }
}