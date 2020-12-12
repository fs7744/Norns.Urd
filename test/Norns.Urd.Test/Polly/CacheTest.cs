using Microsoft.Extensions.DependencyInjection;
using Norns.Urd;
using Norns.Urd.Extensions.Polly;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Test.Norns.Urd.Polly
{
    public class CacheTest
    {
        public class ContextKeyFromCount : IContextKeyGenerator
        {
            public string GenerateKey(AspectContext context)
            {
                return context.Parameters[0].ToString();
            }
        }

        public class DoCacheTest
        {
            public int Count { get; set; }

            [Cache("00:00:01")]
            public virtual void DoVoid(int count)
            {
                Count = count;
            }

            [Cache("00:00:02")]
            public virtual int Do(int count)
            {
                return count;
            }

            [ContextKey(Key = "T")]
            [Cache("00:00:01", TtlStrategy = TtlStrategy.Relative)]
            public virtual Task<int> DoAsync(int count)
            {
                return Task.FromResult(count);
            }

            [ContextKey(GeneratorType = typeof(ContextKeyFromCount))]
            [Cache("00:00:01", TtlStrategy = TtlStrategy.Relative)]
            public virtual Task<int> DoAsync(string key, int count)
            {
                return Task.FromResult(count);
            }
        }

        public DoCacheTest Mock()
        {
            return new ServiceCollection()
                .AddTransient<DoCacheTest>()
                .ConfigureAop(i => i.EnablePolly())
                .BuildServiceProvider()
               .GetRequiredService<DoCacheTest>();
        }

        [Fact]
        public void CacheWhenSyncNoReturnValue()
        {
            var sut = Mock();
            sut.DoVoid(3);
            Assert.Equal(3, sut.Count);
            sut.DoVoid(5);
            Assert.Equal(3, sut.Count);
            Thread.Sleep(1000);
            sut.DoVoid(5);
            Assert.Equal(5, sut.Count);
            sut.DoVoid(3);
            Assert.Equal(5, sut.Count);
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

        [Fact]
        public async Task CacheWhenAsync()
        {
            var sut = Mock();
            Assert.Equal(3, await sut.DoAsync(3));
            Assert.Equal(3, await sut.DoAsync(5));
            Assert.Equal(3, await sut.DoAsync(523));
            Assert.Equal(3, await sut.DoAsync(343));
            Thread.Sleep(1000);
            Assert.Equal(35, await sut.DoAsync(35));
            Assert.Equal(35, await sut.DoAsync(3));
            Assert.Equal(35, await sut.DoAsync(5));
            Assert.Equal(35, await sut.DoAsync(523));
            Assert.Equal(35, await sut.DoAsync(343));
        }

        [Fact]
        public async Task CacheWhenAsyncWithGeneratorType()
        {
            var sut = Mock();
            Assert.Equal(3, await sut.DoAsync("k", 3));
            Assert.Equal(3, await sut.DoAsync("k", 5));
            Assert.Equal(523, await sut.DoAsync("k2", 523));
            Assert.Equal(523, await sut.DoAsync("k2", 343));
            Assert.Equal(3, await sut.DoAsync("k", 533));
            Thread.Sleep(1000);
            Assert.Equal(35, await sut.DoAsync("k", 35));
            Assert.Equal(35, await sut.DoAsync("k", 3));
            Assert.Equal(35, await sut.DoAsync("k", 5));
            Assert.Equal(523, await sut.DoAsync("k2", 523));
            Assert.Equal(523, await sut.DoAsync("k2", 343));
        }
    }
}