using Norns.Urd.DynamicProxy;
using Norns.Urd.Interceptors;
using Norns.Urd.Reflection;
using System;
using System.Reflection;

namespace Norns.Urd
{
    public static class AopInitializer
    {
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
            var field = type.GetField(Constants.Instance, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.CreateSetter();
        }
    }
}