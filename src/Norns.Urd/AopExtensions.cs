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
        public static IServiceCollection ConfigureAop(this IServiceCollection services, Action<IAspectConfiguration> configure = null, IProxyServiceDescriptorConverter converter = null)
        {
            converter ??= InitProxyServiceDescriptorConverter();
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
            services.TryAddSingleton<IInterceptorFactory, InterceptorFactory>();
            return services;
        }

        public static IProxyServiceDescriptorConverter InitProxyServiceDescriptorConverter()
        {
            return new ServiceCollection()
                .AddSingleton<IProxyGenerator, FacadeProxyGenerator>()
                .AddSingleton<IProxyGenerator, InheritProxyGenerator>()
                .AddSingleton<IProxyCreator, ProxyCreator>()
                .AddSingleton<IServiceDescriptorConvertHandler, IngoreServiceDescriptorConvertHandler>()
                .AddSingleton<IServiceDescriptorConvertHandler, ImplementationFactoryServiceDescriptorConvertHandler>()
                .AddSingleton<IServiceDescriptorConvertHandler, SameTypeServiceDescriptorConvertHandler>()
                .AddSingleton<IServiceDescriptorConvertHandler, ImplementationTypeServiceDescriptorConvertHandler>()
                .AddSingleton<IProxyServiceDescriptorConverter, ProxyServiceDescriptorConverter>()
                .BuildServiceProvider()
                .GetRequiredService<IProxyServiceDescriptorConverter>();
        }
    }
}
