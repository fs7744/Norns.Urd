using Norns.Urd;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AopExtensions
    {
        public static IServiceCollection ConfigureAop(this IServiceCollection services, Action<IAspectConfiguration> config = null, bool isIgnoreError = true)
        {
            var (proxyGenerator, configServices, configuration) = config.Init();
            var creator = new ProxyServiceDescriptorCreator(proxyGenerator, configuration);
            foreach (var item in services.ToArray())
            {
                try
                {
                    if (creator.TryCreate(item, out var proxy))
                    {
                        var index = services.IndexOf(item);
                        services.RemoveAt(index);
                        services.Insert(index, proxy);
                    }
                }
                catch (Exception)
                {
                    if (!isIgnoreError)
                    {
                        throw;
                    }
                }
            }
            foreach (var item in configServices)
            {
                item(services);
            }
            return services;
        }
    }
}