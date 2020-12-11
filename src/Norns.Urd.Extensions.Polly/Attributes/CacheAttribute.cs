using Polly;
using Polly.Caching;
using System;

namespace Norns.Urd.Extensions.Polly
{
    public enum TtlStrategy
    {
        Relative,
        Absolute,
        Sliding
    }

    public class CacheAttribute : AbstractLazyPolicyAttribute
    {
        private readonly string timeSpan;
        public TtlStrategy TtlStrategy { get; set; } = TtlStrategy.Sliding;

        public CacheAttribute(string timeSpan)
        {
            this.timeSpan = timeSpan;
        }

        public override ISyncPolicy Build(AspectContext context)
        {
            return Policy.Cache(context.ServiceProvider.GetService(typeof(ISyncCacheProvider)) as ISyncCacheProvider, GetPollyTtlStrategy());
        }

        public override IAsyncPolicy BuildAsync(AspectContext context)
        {
            return Policy.CacheAsync(context.ServiceProvider.GetService(typeof(IAsyncCacheProvider)) as IAsyncCacheProvider, GetPollyTtlStrategy());
        }

        public ITtlStrategy GetPollyTtlStrategy()
        {
            switch (TtlStrategy)
            {
                case TtlStrategy.Relative:
                    return new RelativeTtl(TimeSpan.Parse(timeSpan));

                case TtlStrategy.Absolute:
                    return new AbsoluteTtl(DateTimeOffset.Parse(timeSpan));

                case TtlStrategy.Sliding:
                    return new SlidingTtl(TimeSpan.Parse(timeSpan));

                default:
                    throw new NotSupportedException();
            }
        }
    }
}