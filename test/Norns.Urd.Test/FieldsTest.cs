using Norns.Urd.Proxy;
using System;
using Xunit;

namespace Norns.Urd.UT
{
    public class FieldsTestClass
    {
        private int PrivateInt;
        protected int ProtectedInt;
        protected internal int ProtectedInternalInt;
        protected internal int InternalProtectedInt;
        public int PublicInt;
        public static int PublicStaticInt;
    }

    public class FieldsTest
    {
        private readonly IProxyCreator creator;
        private readonly IInterceptorFactory interceptor;

        public FieldsTest()
        {
            var (c, f, _) = ProxyCreatorUTHelper.InitPorxyCreator();
            creator = c;
            interceptor = f;
        }

        [Fact]
        public void InheritWhenNoConstructorsInterface()
        {
            var proxyType = creator.CreateProxyType(typeof(FieldsTestClass));
            Assert.Equal("FieldsTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { interceptor }) as FieldsTestClass;
            Assert.NotNull(v);
            Assert.Equal(0, v.PublicInt);
            Assert.Equal(0, v.ProtectedInternalInt);
        }
    }
}