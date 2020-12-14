using System;

namespace Norns.Urd.Caching
{
    public class DefaultCacheOptionGenerator : ICacheOptionGenerator
    {
        private readonly Func<AspectContext, CacheOptions> generator;

        public DefaultCacheOptionGenerator(Func<AspectContext, CacheOptions> generator)
        {
            this.generator = generator;
        }

        public CacheOptions Generate(AspectContext context)
        {
            return generator(context);
        }
    }
}