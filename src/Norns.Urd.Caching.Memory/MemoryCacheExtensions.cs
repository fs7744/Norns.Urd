using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Norns.Urd.Caching.Memory
{
    public static class MemoryCacheExtensions
    {
        public static MemoryCacheEntryOptions ToMemoryCacheEntryOptions(this CacheOptions options)
        {
            var op = new MemoryCacheEntryOptions()
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.SlidingExpiration,
                Size = options.Size
            };

            return op;
        }

        public static IAspectConfiguration EnableMemoryCache(this IAspectConfiguration configuration)
        {
            configuration.GlobalInterceptors.TryAdd(() => new CacheInterceptor());
            configuration.ConfigServices.Add(services =>
            {
                services.AddMemoryCache();
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(ICacheAdapter<>), typeof(MemoryCacheAdapter<>)));
                services.TryAddSingleton(typeof(ICacheProvider<>), typeof(CacheProvider<>));
            });
            return configuration;
        }
    }
}