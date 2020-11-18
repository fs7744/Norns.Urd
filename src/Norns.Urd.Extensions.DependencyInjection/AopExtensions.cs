using Norns.Urd;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AopExtensions
    {
        public static IServiceCollection ConfigureAop(this IServiceCollection services, Action<IAspectConfiguration> config = null, bool isIgnoreError = true)
        {
            var creator = new ProxyServiceDescriptorCreator(config.Init());
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
            return services;
        }
    }
}