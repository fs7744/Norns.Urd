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
        public static IServiceCollection ConfigureAop(this IServiceCollection services, IAspectConfiguration config = null)
        {
            config ??= Init(new AspectConfiguration());
            foreach (var item in services.ToArray())
            {
                var proxy = config.Converter.Convert(item);
                if (proxy != null)
                {
                    var index = services.IndexOf(item);
                    services.RemoveAt(index);
                    services.Insert(index, proxy);
                }
            }
            services.TryAddSingleton(config.InterceptorFactory);
            return services;
        }

        public static IAspectConfiguration Init(IAspectConfiguration config)
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
            config.Converter ??= provider.GetRequiredService<IProxyServiceDescriptorConverter>();
            config.InterceptorFactory ??= provider.GetRequiredService<IInterceptorFactory>();
            return config;
        }
    }
}
