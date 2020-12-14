using Microsoft.Extensions.DependencyInjection.Extensions;
using Norns.Urd;
using Norns.Urd.Caching;
using Norns.Urd.Caching.DistributedCache;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DistributedCacheServiceCollectionExtensions
    {
        public static IAspectConfiguration EnableDistributedCacheSerializationAdapter<T>(this IAspectConfiguration configuration, Func<IServiceProvider, T> creator) where T : ISerializationAdapter
        {
            configuration.GlobalInterceptors.TryAdd(() => new CacheInterceptor());
            configuration.ConfigServices.Add(services =>
            {
                services.TryAddSingleton(typeof(T), i => creator(i));
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(ICacheAdapter), typeof(DistributedCacheAdapter<T>)));
                services.TryAddSingleton(typeof(ICacheProvider<>), typeof(CacheProvider<>));
            });
            return configuration;
        }

        public static IAspectConfiguration EnableDistributedCacheSystemTextJsonAdapter(this IAspectConfiguration configuration, string name = "json")
        {
            return configuration.EnableDistributedCacheSerializationAdapter(i => new SystemTextJsonAdapter(name));
        }
    }
}