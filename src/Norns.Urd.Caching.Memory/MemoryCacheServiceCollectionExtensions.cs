using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Norns.Urd;
using Norns.Urd.Caching;
using Norns.Urd.Caching.Memory;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class MemoryCacheServiceCollectionExtensions
    {
        public static IAspectConfiguration EnableMemoryCache(this IAspectConfiguration configuration)
        {
            configuration.GlobalInterceptors.TryAdd(() => new CacheInterceptor());
            configuration.ConfigServices.Add(services =>
            {
                services.AddMemoryCache();
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(ICacheAdapter), typeof(MemoryCacheAdapter)));
                services.TryAddSingleton(typeof(ICacheProvider<>), typeof(CacheProvider<>));
            });
            return configuration;
        }
    }
}