using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Extensions.Polly;
using Polly.Timeout;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Test.Norns.Urd.Polly
{
    public class TimeoutTest
    {
        public class DoTimeoutTest
        {
            [Timeout(10)]
            public virtual int Wait(int milliseconds)
            {
                Thread.Sleep(milliseconds);
                return milliseconds;
            }

            [Timeout("00:00:00.100")]
            public virtual async Task<int> WaitAsync(int milliseconds, CancellationToken cancellationToken = default)
            {
                await Task.Delay(milliseconds, cancellationToken);
                return milliseconds;
            }

            [Timeout("00:00:00.100")]
            public virtual async Task<int> NoCancellationTokenWaitAsync(int milliseconds)
            {
                await Task.Delay(milliseconds);
                return milliseconds;
            }
        }

        public DoTimeoutTest Mock()
        {
            return new ServiceCollection()
                .AddTransient<DoTimeoutTest>()
                .ConfigureAop(i => i.EnablePolly())
                .BuildServiceProvider()
               .GetRequiredService<DoTimeoutTest>();
        }

        [Fact]
        public void TimeoutWhenSync()
        {
            var sut = Mock();
            Assert.Equal(2, sut.Wait(2));
            Assert.Throws<TimeoutRejectedException>(() => sut.Wait(200));
        }

        [Fact]
        public async Task TimeoutWhenAsync()
        {
            var sut = Mock();
            Assert.Equal(4, await sut.WaitAsync(4));
            await Assert.ThrowsAsync<TaskCanceledException>(() => sut.WaitAsync(200));
        }

        [Fact]
        public async Task TimeoutWhenNoCancellationTokenAsyncWillNoWorkAndNoException()
        {
            var sut = Mock();
            Assert.Equal(1, await sut.WaitAsync(1));
            await sut.NoCancellationTokenWaitAsync(200);
        }
    }
}