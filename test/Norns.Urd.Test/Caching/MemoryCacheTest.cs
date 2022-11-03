using Microsoft.Extensions.DependencyInjection;
using Norns.Urd;
using Norns.Urd.Caching;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Test.Norns.Urd.Caching
{
    public class MemoryCacheTest
    {
        public class ContextKeyFromCount : ICacheOptionGenerator
        {
            public CacheOptions Generate(AspectContext context)
            {
                return new CacheOptions()
                {
                    CacheName = CacheOptions.DefaultCacheName,
                    CacheKey = context.Parameters[0],
                    SlidingExpiration = TimeSpan.Parse("00:00:01")
                };
            }
        }

        public interface IDoCacheTest 
        {
            [Cache(nameof(DoInterfaceint), AbsoluteExpirationRelativeToNow = "00:00:02")]
            int DoInterfaceint(int count);
        }

        public class DoCacheTest : IDoCacheTest
        {
            public int Count { get; set; }

            [Cache(nameof(DoVoid), AbsoluteExpiration = "00:00:01")]
            public virtual void DoVoid(int count)
            {
                Count = count;
            }

            [Cache(nameof(Do), AbsoluteExpirationRelativeToNow = "00:00:02")]
            public virtual int Do(int count)
            {
                return count;
            }

            [Cache("T", SlidingExpiration = "00:00:01")]
            public virtual Task<int> DoAsync(int count)
            {
                return Task.FromResult(count);
            }

            [Cache(typeof(ContextKeyFromCount))]
            public virtual Task<int> DoAsync(string key, int count)
            {
                return Task.FromResult(count);
            }

            [Cache(typeof(ContextKeyFromCount))]
            public virtual Task DoTaskAsync(int count)
            {
                Count = count;
                return Task.CompletedTask;
            }

            [Cache(typeof(ContextKeyFromCount))]
            public virtual ValueTask DoValueTaskAsync(int count)
            {
                Count = count;
                return new ValueTask(Task.CompletedTask);
            }

            public int DoInterfaceint(int count)
            {
                return count;
            }
        }

        public T Mock<T>() where T : IDoCacheTest
        {
            return new ServiceCollection()
                .AddTransient<DoCacheTest>()
                .AddTransient<IDoCacheTest, DoCacheTest>()
                .ConfigureAop(i => i.EnableMemoryCache())
                .BuildServiceProvider()
               .GetRequiredService<T>();
        }

        public DoCacheTest Mock()
        {
            return Mock<DoCacheTest>();
        }

        [Fact]
        public void CacheWhenInterfaceSyncNoReturnValue()
        {
            var sut = Mock<IDoCacheTest>();
            Assert.Equal(3, sut.DoInterfaceint(3));
            Assert.Equal(3, sut.DoInterfaceint(5));
        }

        [Fact]
        public void CacheWhenSyncNoReturnValue()
        {
            var sut = Mock();
            sut.DoVoid(3);
            Assert.Equal(3, sut.Count);
            sut.DoVoid(5);
            Assert.Equal(5, sut.Count);
        }

        [Fact]
        public async Task CacheWhenTaskAsyncNoReturnValue()
        {
            var sut = Mock();
            await sut.DoTaskAsync(3);
            Assert.Equal(3, sut.Count);
            await sut.DoTaskAsync(5);
            Assert.Equal(5, sut.Count);
        }

        [Fact]
        public async Task CacheWhenValueTaskAsyncNoReturnValue()
        {
            var sut = Mock();
            await sut.DoValueTaskAsync(3);
            Assert.Equal(3, sut.Count);
            await sut.DoValueTaskAsync(5);
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
            await Task.Delay(1500);
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
            await Task.Delay(1500);
            Assert.Equal(35, await sut.DoAsync("k", 35));
            Assert.Equal(35, await sut.DoAsync("k", 3));
            Assert.Equal(35, await sut.DoAsync("k", 5));
            Assert.Equal(523, await sut.DoAsync("k2", 523));
            Assert.Equal(523, await sut.DoAsync("k2", 343));
        }
    }
}