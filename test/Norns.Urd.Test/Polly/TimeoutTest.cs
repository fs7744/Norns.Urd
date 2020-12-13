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
            [Timeout(2)]
            public virtual int Wait(int seconds)
            {
                Thread.Sleep(seconds * 1000);
                return seconds;
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
            Assert.Equal(1, sut.Wait(1));
            Assert.Throws<TimeoutRejectedException>(() => sut.Wait(3));
        }

        [Fact]
        public async Task TimeoutWhenAsync()
        {
            var sut = Mock();
            Assert.Equal(4, await sut.WaitAsync(4));
            await Assert.ThrowsAsync<TaskCanceledException>(() => sut.WaitAsync(500));
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