using System;

namespace Norns.Urd.Caching
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true)]
    public class CacheAttribute : Attribute
    {
        private readonly ICacheOptionGenerator optionGenerator;
        public int Order { get; set; }

        private DateTimeOffset? absoluteExpiration;

        //     Gets or sets an absolute expiration date for the cache entry.
        public string AbsoluteExpiration { get => absoluteExpiration?.ToString(); set => absoluteExpiration = DateTimeOffset.Parse(value); }

        private TimeSpan? absoluteExpirationRelativeToNow;

        //     Gets or sets an absolute expiration time, relative to now.
        public string AbsoluteExpirationRelativeToNow { get => absoluteExpirationRelativeToNow?.ToString(); set => absoluteExpirationRelativeToNow = TimeSpan.Parse(value); }

        private TimeSpan? slidingExpiration;

        //     Gets or sets how long a cache entry can be inactive (e.g. not accessed) before
        //     it will be removed. This will not extend the entry lifetime beyond the absolute
        //     expiration (if set).
        public string SlidingExpiration { get => slidingExpiration?.ToString(); set => slidingExpiration = TimeSpan.Parse(value); }

        public long? Size { get; set; }

        public CacheAttribute(string cacheKey, string cacheName = CacheOptions.DefaultCacheName)
        {
            if (cacheKey == null)
            {
                throw new ArgumentNullException(nameof(cacheKey));
            }
            if (cacheName == null)
            {
                throw new ArgumentNullException(nameof(cacheName));
            }
            optionGenerator = new DefaultCacheOptionGenerator(c => new CacheOptions()
            {
                CacheName = cacheName,
                CacheKey = cacheKey,
                AbsoluteExpiration = absoluteExpiration,
                AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow,
                SlidingExpiration = slidingExpiration,
                Size = Size
            });
        }

        public CacheAttribute(Type cacheOptionGeneratorType)
        {
            optionGenerator = Activator.CreateInstance(cacheOptionGeneratorType) as ICacheOptionGenerator;
            if (optionGenerator == null)
            {
                throw new ArgumentException("cacheOptionGeneratorType must be ICacheOptionGenerator type.");
            }
        }

        public Func<AspectContext, CacheOptions> GetCacheOptionCreator()
        {
            return optionGenerator.Generate;
        }
    }
}