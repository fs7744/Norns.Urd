using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Norns.Urd.IOC;
using Norns.Urd.Proxy;
using System;
using System.Linq;

namespace Norns.Urd
{
    public static class AopExtensions
    {
        public static IServiceCollection ConfigureAop(this IServiceCollection services, Action<IAspectConfiguration> doConfig = null)
        {
            var config = new AspectConfiguration();
            doConfig?.Invoke(config);
            var (converter, interceptorFactory) = Init(config);
            foreach (var item in services.ToArray())
            {
                var proxy = converter.Convert(item);
                if (proxy != null)
                {
                    var index = services.IndexOf(item);
                    services.RemoveAt(index);
                    services.Insert(index, proxy);
                }
            }
            services.TryAddSingleton(interceptorFactory);
            return services;
        }

        public static (IProxyServiceDescriptorConverter, IInterceptorFactory) Init(IAspectConfiguration config)
        {
            var provider = new ServiceCollection()
                .AddSingleton<IProxyGenerator, FacadeProxyGenerator>()
                .AddSingleton<IProxyGenerator, InheritProxyGenerator>()
                .AddSingleton<IProxyCreator, ProxyCreator>()
                .AddSingleton<IInterceptorFactory, InterceptorFactory>()
                .AddSingleton(config)
                .AddSingleton<IServiceDescriptorConvertHandler, IngoreServiceDescriptorConvertHandler>()
                .AddSingleton<IServiceDescriptorConvertHandler, ImplementationFactoryServiceDescriptorConvertHandler>()
                .AddSingleton<IServiceDescriptorConvertHandler, SameTypeServiceDescriptorConvertHandler>()
                .AddSingleton<IServiceDescriptorConvertHandler, ImplementationTypeServiceDescriptorConvertHandler>()
                .AddSingleton<IProxyServiceDescriptorConverter, ProxyServiceDescriptorConverter>()
                .BuildServiceProvider();
            return (provider.GetRequiredService<IProxyServiceDescriptorConverter>(), provider.GetRequiredService<IInterceptorFactory>());
        }
    }
}
