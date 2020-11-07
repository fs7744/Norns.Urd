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
    }

    //public class SubMethodTestClass : MethodTestClass
    //{
    //    public override int NoArgsReturnInt() => base.NoArgsReturnInt();
    //}

    public class MethodTest
    {
        private readonly IProxyCreator creator;
        private readonly IInterceptorFactory interceptor;

        public MethodTest()
        {
            var (c, f, conf) = ProxyCreatorUTHelper.InitPorxyCreator();
            creator = c;
            interceptor = f;
            conf.Interceptors.Add(new TestInterceptor());
        }

        [Fact]
        public void WhenPublicMethod()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { interceptor }) as MethodTestClass;
            Assert.NotNull(v);
            v.NoArgsVoid();
        }

        [Fact]
        public void WhenSubMethodTestClass()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { interceptor }) as MethodTestClass;
            Assert.NotNull(v);
            Assert.Equal(13, v.NoArgsReturnInt());
        }
    }
}