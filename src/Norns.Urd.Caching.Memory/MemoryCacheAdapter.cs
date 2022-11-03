using Microsoft.Extensions.Caching.Memory;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Caching.Memory
{
    public class MemoryCacheAdapter : ICacheAdapter
    {
        private readonly IMemoryCache cache;

        public MemoryCacheAdapter(IMemoryCache cache)
        {
            this.cache = cache;
        }

        public string Name => CacheOptions.DefaultCacheName;

        public void Set<T>(CacheOptions op, T result)
        {
            cache.Set(op.CacheKey, result, op.ToMemoryCacheEntryOptions());
        }

        public Task SetAsync<T>(CacheOptions op, T result, CancellationToken token)
        {
            Set(op, result);
            return Task.CompletedTask;
        }

        public bool TryGetValue<T>(CacheOptions op, out T result)
        {
            return cache.TryGetValue(op.CacheKey, out result);
        }

        public Task<(bool, T)> TryGetValueAsync<T>(CacheOptions op, CancellationToken token)
        {
            var hasValue = cache.TryGetValue<T>(op.CacheKey, out var result);
            return Task.FromResult((hasValue, result));
        }
    }
}