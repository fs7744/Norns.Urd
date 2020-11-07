using Norns.Urd;
using Norns.Urd.IOC;
using Norns.Urd.Proxy;
using Norns.Urd.Utils;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AopExtensions
    {
        public static IServiceCollection ConfigureAop(this IServiceCollection services, Action<IAspectConfiguration> doConfig = null)
        {
            var config = new AspectConfiguration();
            doConfig?.Invoke(config);
            var converter = Init(config);
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
            return services;
        }

        public static IProxyServiceDescriptorConverter Init(IAspectConfiguration config)
        {
            var provider = new ServiceCollection()
                .AddSingleton<IProxyGenerator, FacadeProxyGenerator>()
                .AddSingleton<IProxyGenerator, InheritProxyGenerator>()
                .AddSingleton<IProxyCreator, ProxyCreator>()
                .AddSingleton<IInterceptorFactory, InterceptorFactory>()
                .AddSingleton<ConstantInfo>()
                .AddSingleton(config)
                .AddSingleton<IServiceDescriptorConvertHandler, IngoreServiceDescriptorConvertHandler>()
                .AddSingleton<IServiceDescriptorConvertHandler, ImplementationFactoryServiceDescriptorConvertHandler>()
                .AddSingleton<IServiceDescriptorConvertHandler, SameTypeServiceDescriptorConvertHandler>()
                .AddSingleton<IServiceDescriptorConvertHandler, ImplementationTypeServiceDescriptorConvertHandler>()
                .AddSingleton<IProxyServiceDescriptorConverter, ProxyServiceDescriptorConverter>()
                .BuildServiceProvider();
            return provider.GetRequiredService<IProxyServiceDescriptorConverter>();
        }
    }
}