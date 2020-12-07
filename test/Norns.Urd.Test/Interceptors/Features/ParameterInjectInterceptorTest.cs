using Microsoft.Extensions.DependencyInjection;
using Norns.Urd;
using Norns.Urd.Reflection;
using System.Threading.Tasks;
using Test.Norns.Urd.DependencyInjection;
using Xunit;

namespace Test.Norns.Urd.Interceptors.Features
{
    public class ParameterInjectTest : IInjectTest
    {
        [Inject]
        ParameterInjectInterceptorTest FT;

        [Inject]
        public ParameterInjectInterceptorTest FT1;

        public ParameterInjectInterceptorTest FT2 => FT;

        [Inject]
        public ParameterInjectInterceptorTest PT { get; set; }

        [Inject]
        public virtual ParameterInjectInterceptorTest PT2 { get; set; }

        [Inject]
        private ParameterInjectInterceptorTest PT4 { get; set; }

        public ParameterInjectInterceptorTest PT3 => PT4;

        public virtual ParameterInjectTest T([Inject] ParameterInjectTest t) => t;

        public virtual Task<ParameterInjectTest> T(int i, long y, [Inject] ParameterInjectTest t, object ii) => Task.FromResult(t);
    }

    public interface IInjectTest
    {
        [Inject]
        ParameterInjectInterceptorTest PT { get; set; }

        public ParameterInjectInterceptorTest T([Inject] ParameterInjectInterceptorTest t = null) => t;
    }

    public class ParameterInjectInterceptorTest
    {
        [Fact]
        public async Task WhenHasParameterInject()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<ParameterInjectTest>().AddTransient<ParameterInjectInterceptorTest>())
                    .GetRequiredService<ParameterInjectTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(p.T(null));
            Assert.NotNull(await p.T(3, 4, null, null));
            var a = new ParameterInjectTest();
            Assert.Same(a, p.T(a));
            Assert.NotNull(p.PT);
            Assert.NotNull(p.PT2);
            Assert.NotNull(p.PT3);
            Assert.NotNull(p.FT1);
            Assert.NotNull(p.FT2);
        }

        [Fact]
        public void WhenInterfaceHasParameterInject()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IInjectTest>().AddTransient<ParameterInjectInterceptorTest>())
                    .GetRequiredService<IInjectTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(p.T());
            Assert.Same(this, p.T(this));
            Assert.Null(p.PT);

            p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IInjectTest, ParameterInjectTest>().AddTransient<ParameterInjectInterceptorTest>())
                    .GetRequiredService<IInjectTest>();
            pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(p.T());
            Assert.Same(this, p.T(this));
            Assert.NotNull(p.PT);
        }
    }
}