using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Extensions.Polly;
using Polly.Caching;
using Polly.Caching.Memory;
using System.Threading;
using Xunit;

namespace Test.Norns.Urd.Polly
{
    public class CacheTest
    {
        public class DoCacheTest
        {
            [Cache("00:00:02")]
            public virtual int Do(int count)
            {
                return count;
            }
        }

        public DoCacheTest Mock()
        {
            return new ServiceCollection()
                .AddTransient<DoCacheTest>()
                .AddMemoryCache()
                .ConfigureAop(i => i.EnablePolly())
                .AddSingleton<ISyncCacheProvider>(i => new MemoryCacheProvider(i.GetRequiredService<IMemoryCache>()))
                .AddSingleton<IAsyncCacheProvider>(i => i.GetRequiredService<ISyncCacheProvider>() as IAsyncCacheProvider)
                .BuildServiceProvider()
               .GetRequiredService<DoCacheTest>();
        }

        [Fact]
        public void CacheWhenSync()
        {
            var sut = Mock();
            Assert.Equal(3, sut.Do(3));
            Assert.Equal(3, sut.Do(5));
            Assert.Equal(3, sut.Do(523));
            Assert.Equal(3, sut.Do(343));
            Thread.Sleep(2000);
            Assert.Equal(35, sut.Do(35));
            Assert.Equal(35, sut.Do(3));
            Assert.Equal(35, sut.Do(5));
            Assert.Equal(35, sut.Do(523));
            Assert.Equal(35, sut.Do(343));
        }

    }
}