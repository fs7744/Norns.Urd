using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Norns.Urd;
using Norns.Urd.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static Test.Norns.Urd.DependencyInjection.ExceptionTest;

namespace Test.Norns.Urd.DependencyInjection
{
    [NonAspect]
    public class AutofacTest
    {
        [Fact]
        public void WhenUseAutofac()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(AopTestExtensions.ConfigCollectionWithAop(i => i.AddSingleton(i => new OneConstructorOneStringArgs("a"))));
            var v = new AutofacServiceProvider(containerBuilder.Build())
                .GetRequiredService<OneConstructorOneStringArgs>();
            var pt = v.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(pt.CreateInstanceGetter()(v));
            Assert.NotNull(pt.CreateServiceProviderGetter()(v));
            Assert.NotNull(v);
            Assert.NotNull(v.A);
            Assert.Equal("a", v.A);
        }

        public DoExceptionTest Mock()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.Populate(new ServiceCollection()
                .AddTransient<DoExceptionTest>()
                .ConfigureAop(i => i.GlobalInterceptors.Add(new NoThings())));
            return new AutofacServiceProvider(containerBuilder.Build())
           .GetRequiredService<DoExceptionTest>();
        }

        [Fact]
        public async Task WhenExceptionFromMehtodUseAutofac()
        {
            var sut = Mock();
            Assert.Throws<FieldAccessException>(() => sut.DoVoid(true));
            Assert.Throws<FieldAccessException>(() => sut.Do(true));
            await Assert.ThrowsAsync<FieldAccessException>(() => sut.DoAsync(true));
            await Assert.ThrowsAsync<FieldAccessException>(async () => await sut.DoErrorValueTaskAsync(true));
            await Assert.ThrowsAsync<FieldAccessException>(() => sut.DoValueTaskAsync(true).AsTask());
            await Assert.ThrowsAsync<FieldAccessException>(() => sut.DoTaskAsync(true));
        }
    }
}
