using Microsoft.Extensions.DependencyInjection;
using Norns.Urd;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Test.Norns.Urd.Interceptors.Features
{
    public class FallbackAttributeTest
    {
        public class TestFallback : AbstractInterceptor
        {
            public override void Invoke(AspectContext context, AspectDelegate next)
            {
                context.ReturnValue = (int)context.Parameters[0];
            }

            public override Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
            {
                var t = Task.FromResult((int)context.Parameters[0]);
                context.ReturnValue = t;
                return t;
            }
        }

        public class DoFallbackTest
        {
            [Fallback(typeof(TestFallback))]
            public virtual int Do(int i)
            {
                throw new FieldAccessException();
            }

            [Fallback(typeof(TestFallback))]
            public virtual Task<int> DoAsync(int i)
            {
                throw new FieldAccessException();
            }
        }

        public DoFallbackTest Mock()
        {
            return new ServiceCollection()
                .AddTransient<DoFallbackTest>()
                .ConfigureAop()
                .BuildServiceProvider()
               .GetRequiredService<DoFallbackTest>();
        }

        [Fact]
        public void RetryWhenSync()
        {
            var sut = Mock();
            Assert.Equal(4, sut.Do(4));
        }

        [Fact]
        public async Task RetryWhenAsync()
        {
            var sut = Mock();
            Assert.Equal(3, await sut.DoAsync(3));
        }
    }
}