using Microsoft.Extensions.Caching.Distributed;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Caching.DistributedCache
{
    public class DistributedCacheAdapter<R> : ICacheAdapter where R : ISerializationAdapter
    {
        private readonly IDistributedCache cache;
        private readonly R serializer;

        public DistributedCacheAdapter(IDistributedCache cache, R serializer)
        {
            this.cache = cache;
            this.serializer = serializer;
        }

        public string Name => serializer.Name;

        public void Set<T>(CacheOptions op, T result)
        {
            cache.Set(op.CacheKey.ToString(), serializer.Serialize(result), op.ToDistributedCacheEntryOptions());
        }

        public async Task SetAsync<T>(CacheOptions op, T result, CancellationToken token)
        {
            await cache.SetAsync(op.CacheKey.ToString(), serializer.Serialize(result), op.ToDistributedCacheEntryOptions(), token);
        }

        public bool TryGetValue<T>(CacheOptions op, out T result)
        {
            var data = cache.Get(op.CacheKey.ToString());
            if (data == null)
            {
                result = default;
                return false;
            }
            else
            {
                result = serializer.Deserialize<T>(data);
                return true;
            }
        }

        public async Task<(bool, T)> TryGetValueAsync<T>(CacheOptions op, CancellationToken token)
        {
            var data = await cache.GetAsync(op.CacheKey.ToString(), token);
            if (data == null)
            {
                return (false, default);
            }
            else
            {
                return (true, serializer.Deserialize<T>(data));
            }
        }
    }
}