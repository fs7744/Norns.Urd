using Norns.Urd.DynamicProxy;
using Norns.Urd.Interceptors;
using System;

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
    }
}