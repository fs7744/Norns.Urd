using Norns.Urd;
using Norns.Urd.Interceptors;
using System;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AopExtensions
    {
        public static IServiceCollection ConfigureAop(this IServiceCollection services, Action<IInterceptorConfiguration> config = null)
        {
            var creator = new ProxyServiceDescriptorCreator(config.Init());
            foreach (var item in services.ToArray())
            {
                if (creator.TryCreate(item, out var proxy))
                {
                    var index = services.IndexOf(item);
                    services.RemoveAt(index);
                    services.Insert(index, proxy);
                }
            }
            return services;
        }
    }
}