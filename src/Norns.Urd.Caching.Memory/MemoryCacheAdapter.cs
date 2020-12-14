using Microsoft.Extensions.Caching.Memory;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Caching.Memory
{
    public class MemoryCacheAdapter<T> : ICacheAdapter<T>
    {
        private readonly IMemoryCache cache;

        public MemoryCacheAdapter(IMemoryCache cache)
        {
            this.cache = cache;
        }

        public string Name => CacheOptions.DefaultCacheName;

        public void Set(CacheOptions op, T result)
        {
            cache.Set(op.CacheKey, result, op.ToMemoryCacheEntryOptions());
        }

        public Task SetAsync(CacheOptions op, T result, CancellationToken token)
        {
            Set(op, result);
            return Task.CompletedTask;
        }

        public bool TryGetValue(CacheOptions op, out T result)
        {
            return cache.TryGetValue(op.CacheKey, out result);
        }

        public ValueTask<bool> TryGetValueAsync(CacheOptions op, out T result)
        {
            return new ValueTask<bool>(cache.TryGetValue(op.CacheKey, out result));
        }
    }
}