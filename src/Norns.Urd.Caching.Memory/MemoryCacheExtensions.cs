using Microsoft.Extensions.Caching.Memory;

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
    }
}