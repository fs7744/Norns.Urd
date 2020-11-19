using Microsoft.Extensions.DependencyInjection;
using Norns.Urd;
using Norns.Urd.Reflection;
using System.Threading.Tasks;
using Test.Norns.Urd.DependencyInjection;
using Xunit;

namespace Test.Norns.Urd.Interceptors.Features
{
    public class ParameterInjectTest
    {
        public virtual ParameterInjectTest T([Inject] ParameterInjectTest t) => t;

        public virtual Task<ParameterInjectTest> T(int i, long y, [Inject] ParameterInjectTest t, object ii) => Task.FromResult(t);
    }

    public class ParameterInjectInterceptorTest
    {
        [Fact]
        public async Task WhenHasParameterInject()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<ParameterInjectTest>())
                    .GetRequiredService<ParameterInjectTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(p.T(null));
            Assert.NotNull(await p.T(3, 4, null, null));
        }
    }
}