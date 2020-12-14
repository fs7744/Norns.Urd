using System;
using System.Collections.Generic;
using System.Linq;
using Norns.Urd;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Test.Norns.Urd.DependencyInjection
{
    public class ExceptionTest
    {
        public class NoThings : AbstractInterceptor
        {
            public override async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
            {
                if ((bool)context.Parameters[0])
                {
                    await next(context);
                }
                else
                {
                    throw new FieldAccessException();
                }
            }
        }

        public class DoExceptionTest
        {
            public virtual void DoVoid(bool error)
            {
                if (error)
                {
                    throw new FieldAccessException();
                }
            }

            public virtual bool Do(bool error)
            {
                if (error)
                {
                    throw new FieldAccessException();
                }
                return error;
            }

            public virtual Task<bool> DoAsync(bool error)
            {
                if (error)
                {
                    throw new FieldAccessException();
                }
                return Task.FromResult(error);
            }

            public virtual Task DoTaskAsync(bool error)
            {
                if (error)
                {
                    throw new FieldAccessException();
                }
                return Task.CompletedTask;
            }

            public virtual ValueTask DoValueTaskAsync(bool error)
            {
                if (error)
                {
                    throw new FieldAccessException();
                }
                return new ValueTask(Task.CompletedTask);
            }

            public virtual ValueTask<bool> DoErrorValueTaskAsync(bool error)
            {
                if (error)
                {
                    throw new FieldAccessException();
                }
                return new ValueTask<bool>(error);
            }
        }

        public DoExceptionTest Mock()
        {
            return new ServiceCollection()
                .AddTransient<DoExceptionTest>()
                .ConfigureAop(i => i.GlobalInterceptors.Add(new NoThings()))
                .BuildServiceProvider()
               .GetRequiredService<DoExceptionTest>();
        }

        [Fact]
        public async Task WhenExceptionFromMehtod()
        {
            var sut = Mock();
            Assert.Throws<FieldAccessException>(() => sut.DoVoid(true));
            Assert.Throws<FieldAccessException>(() => sut.Do(true));
            await Assert.ThrowsAsync<FieldAccessException>(() => sut.DoAsync(true));
            await Assert.ThrowsAsync<FieldAccessException>(async () => await sut.DoErrorValueTaskAsync(true));
            await Assert.ThrowsAsync<FieldAccessException>(() => sut.DoValueTaskAsync(true).AsTask());
            await Assert.ThrowsAsync<FieldAccessException>(() => sut.DoTaskAsync(true));
        }

        [Fact]
        public async Task WhenExceptionFromInterceptor()
        {
            var sut = Mock();
            Assert.Throws<FieldAccessException>(() => sut.DoVoid(false));
            Assert.Throws<FieldAccessException>(() => sut.Do(false));
            await Assert.ThrowsAsync<FieldAccessException>(() => sut.DoAsync(false));
            await Assert.ThrowsAsync<FieldAccessException>(async () => await sut.DoErrorValueTaskAsync(false));
            await Assert.ThrowsAsync<FieldAccessException>(() => sut.DoValueTaskAsync(false).AsTask());
            await Assert.ThrowsAsync<FieldAccessException>(() => sut.DoTaskAsync(false));
        }
    }
}
