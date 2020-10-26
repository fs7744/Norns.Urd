using Norns.Urd.Proxy;
using System;
using Xunit;

namespace Norns.Urd.UT
{
    public class MethodTestClass
    {
        public virtual void NoArgsVoid()
        {
        }
    }

    public class MethodTest
    {
        private readonly IProxyCreator creator = ProxyCreatorUTHelper.InitPorxyCreator();

        [Fact]
        public void WhenPublicMethod()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { new TestInterceptorFactory() }) as MethodTestClass;
            Assert.NotNull(v);
            v.NoArgsVoid();
        }
    }
}