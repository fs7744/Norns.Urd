using Norns.Urd.Proxy;
using System;
using System.Reflection;
using Xunit;

namespace Norns.Urd.UT
{
    public class TestDataAttribute : Attribute
    {
        public TestDataAttribute(int v)
        {
            V = v;
        }

        public int V { get; }
    }

    public interface INoConstructorsInterface
    {
    }

    public class OneConstructorNoArgs
    {
    }

    public class OneConstructorOneArgs
    {
        public readonly int A;

        public OneConstructorOneArgs(int a)
        {
            A = a;
        }
    }

    [TestData(2)]
    public class TwoConstructorOneArgs
    {
        public readonly int A;

        [TestData(99)]
        public TwoConstructorOneArgs([TestData(4)] int a)
        {
            A = a;
        }

        public TwoConstructorOneArgs()
        {
        }
    }

    public abstract class AbstractTwoConstructorOneArgs
    {
        public readonly int A;

        protected AbstractTwoConstructorOneArgs(int a)
        {
            A = a;
        }

        protected AbstractTwoConstructorOneArgs()
        {
        }
    }

    public class ConstructorsTest
    {
        private readonly IProxyCreator creator = ProxyCreatorUTHelper.InitPorxyCreator();

        [Fact]
        public void InheritWhenNoConstructorsInterface()
        {
            var proxyType = creator.CreateProxyType(typeof(INoConstructorsInterface));
            Assert.Equal("INoConstructorsInterface_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { new TestInterceptorFactory() }) as INoConstructorsInterface;
            Assert.NotNull(v);
        }

        [Fact]
        public void InheritWhenOneConstructorNoArgs()
        {
            var proxyType = creator.CreateProxyType(typeof(OneConstructorNoArgs));
            Assert.Equal("OneConstructorNoArgs_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { new TestInterceptorFactory() }) as OneConstructorNoArgs;
            Assert.NotNull(v);
        }

        [Fact]
        public void InheritWhenOneConstructorOneArgs()
        {
            var proxyType = creator.CreateProxyType(typeof(OneConstructorOneArgs));
            Assert.Equal("OneConstructorOneArgs_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { new TestInterceptorFactory(), 4 }) as OneConstructorOneArgs;
            Assert.NotNull(v);
            Assert.Equal(4, v.A);
        }

        [Fact]
        public void InheritWhenTwoConstructorOneArgsAndCustomAttribute()
        {
            var proxyType = creator.CreateProxyType(typeof(TwoConstructorOneArgs));
            Assert.Equal("TwoConstructorOneArgs_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { new TestInterceptorFactory(), 4 }) as TwoConstructorOneArgs;
            Assert.NotNull(v);
            Assert.Equal(4, v.A);
            Assert.Equal(2, proxyType.GetCustomAttribute<TestDataAttribute>().V);
            Assert.Equal(99, proxyType.GetConstructors()[0].GetCustomAttribute<TestDataAttribute>().V);
            Assert.Equal(4, proxyType.GetConstructors()[0].GetParameters()[1].GetCustomAttribute<TestDataAttribute>().V);
            v = Activator.CreateInstance(proxyType, new object[] { new TestInterceptorFactory()}) as TwoConstructorOneArgs;
            Assert.NotNull(v);
            Assert.Equal(0, v.A);
        }

        [Fact]
        public void InheritWhenAbstractTwoConstructorOneArgs()
        {
            var proxyType = creator.CreateProxyType(typeof(AbstractTwoConstructorOneArgs));
            Assert.Equal("AbstractTwoConstructorOneArgs_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { new TestInterceptorFactory(), 4 }) as AbstractTwoConstructorOneArgs;
            Assert.NotNull(v);
            Assert.Equal(4, v.A);
            v = Activator.CreateInstance(proxyType, new object[] { new TestInterceptorFactory() }) as AbstractTwoConstructorOneArgs;
            Assert.NotNull(v);
            Assert.Equal(0, v.A);
        }
    }
}