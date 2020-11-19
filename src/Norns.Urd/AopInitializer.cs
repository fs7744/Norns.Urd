using Norns.Urd.DynamicProxy;
using Norns.Urd.Interceptors.Features;
using Norns.Urd.Reflection;
using System;
using System.Reflection;

namespace Norns.Urd
{
    public static class AopInitializer
    {
        public static IProxyGenerator Init(this Action<IAspectConfiguration> config)
        {
            var configuration = new AspectConfiguration();
            configuration.AddParameterInject();
            config?.Invoke(configuration);
            return new ProxyGenerator(configuration);
        }

        public static Func<object, object> CreateInstanceGetter(this Type type)
        {
            var field = type.GetField(Constants.Instance, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.CreateGetter<object, object>();
        }

        public static Action<object, object> CreateInstanceSetter(this Type type)
        {
            var field = type.GetField(Constants.Instance, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.CreateSetter();
        }

        public static Func<object, IServiceProvider> CreateServiceProviderGetter(this Type type)
        {
            var field = type.GetField(Constants.ServiceProvider, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.CreateGetter<object, IServiceProvider>();
        }
    }
}