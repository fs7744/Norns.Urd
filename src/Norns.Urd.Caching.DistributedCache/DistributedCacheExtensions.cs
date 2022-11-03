using Microsoft.Extensions.Caching.Distributed;

namespace Norns.Urd.Caching.DistributedCache
{
    public static class DistributedCacheExtensions
    {
        public static DistributedCacheEntryOptions ToDistributedCacheEntryOptions(this CacheOptions options)
        {
            var op = new DistributedCacheEntryOptions()
            {
                AbsoluteExpiration = options.AbsoluteExpiration,
                AbsoluteExpirationRelativeToNow = options.AbsoluteExpirationRelativeToNow,
                SlidingExpiration = options.SlidingExpiration,
            };

            return op;
        }
    }
}