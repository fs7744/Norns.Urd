using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Extensions.Polly;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Test.Norns.Urd.Polly
{
    public class RetryTest
    {
        public class DoRetryTest
        {
            public int Count { get; set; }

            [Retry(2)]
            public virtual void Do()
            {
                if (Count < 50)
                {
                    Count++;
                    throw new FieldAccessException();
                }
            }

            [Retry(2, ExceptionType = typeof(FieldAccessException))]
            public virtual Task DoAsync()
            {
                if (Count < 50)
                {
                    Count++;
                    throw new FieldAccessException();
                }

                return Task.CompletedTask;
            }

            [Retry(2, ExceptionType = typeof(AccessViolationException))]
            public virtual void Do2()
            {
                if (Count < 50)
                {
                    Count++;
                    throw new FieldAccessException();
                }
            }
        }

        public static DoRetryTest Mock()
        {
            return new ServiceCollection()
                .AddTransient<DoRetryTest>()
                .ConfigureAop(i => i.EnablePolly())
                .BuildServiceProvider()
               .GetRequiredService<DoRetryTest>();
        }

        [Fact]
        public void RetryWhenSync()
        {
            var sut = Mock();
            Assert.Throws<FieldAccessException>(() => sut.Do());
            Assert.Equal(3, sut.Count);
        }

        [Fact]
        public async Task RetryWhenAsync()
        {
            var sut = Mock();
            await Assert.ThrowsAsync<FieldAccessException>(() => sut.DoAsync());
            Assert.Equal(3, sut.Count);
        }

        [Fact]
        public void RetryWhenNoMatchException()
        {
            var sut = Mock();
            Assert.Throws<FieldAccessException>(() => sut.Do2());
            Assert.Equal(1, sut.Count);
        }
    }
}