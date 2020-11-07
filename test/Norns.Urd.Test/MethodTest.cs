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

        public virtual int NoArgsReturnInt() => 3;

        public virtual int HasArgsReturnInt(int v) => v;
    }

    public class MethodTest
    {
        private readonly IProxyCreator creator;

        public MethodTest()
        {
            var (c, _, conf) = ProxyCreatorUTHelper.InitPorxyCreator();
            creator = c;
            conf.Interceptors.Add(new TestInterceptor());
        }

        [Fact]
        public void WhenPublicMethod()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType) as MethodTestClass;
            Assert.NotNull(v);
            v.NoArgsVoid();
        }

        [Fact]
        public void WhenSubMethodTestClass()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType) as MethodTestClass;
            Assert.NotNull(v);
            Assert.Equal(13, v.NoArgsReturnInt());
        }

        [Fact]
        public void WhenHasArgsReturnInt()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType) as MethodTestClass;
            Assert.NotNull(v);
            Assert.Equal(17, v.HasArgsReturnInt(7));
            Assert.Equal(19, v.HasArgsReturnInt(9));
        }
    }
}