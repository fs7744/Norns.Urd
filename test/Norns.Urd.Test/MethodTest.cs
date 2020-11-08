using Moq;
using Norns.Urd.Proxy;
using System;
using System.Reflection;
using Xunit;

namespace Norns.Urd.UT
{
    public class MethodTestClass
    {
        public virtual void NoArgsVoid()
        {
        }

        public virtual int NoArgsReturnInt() => 3;

        public int NotVirtualNoArgsReturnInt() => 3;

        public virtual int HasArgsReturnInt(int v) => v;

        public virtual (int, long) HasArgsReturnTuple(int v, ref long y) => (v, y);

        public virtual object[] HasArgsReturnArray(int v, out long y)
        {
            y = 99;
            return new object[] { v, y };
        }
    }

    public interface IMethodTestInterface
    {
        MethodTestClass Do();

        public MethodTestClass Do2() => new MethodTestClass();
    }

    public abstract class AbstractMethodTestClass
    {
        public abstract MethodTestClass Do();

        public virtual MethodTestClass Do2() => new MethodTestClass();

        public int Do3() => 3;

        public virtual int Do4() => 3;

        protected virtual int Do5() => 5;
    }

    public class SubClass : AbstractMethodTestClass
    {
        public override MethodTestClass Do() => new MethodTestClass();

        protected override int Do5() => base.Do5() + 5;

        public int CallDo5() => Do5();
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
        public void SubClassWhenHasOverrideMethod()
        {
            var proxyType = creator.CreateProxyType(typeof(SubClass), ProxyTypes.Inherit);
            Assert.Equal("SubClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType) as SubClass;
            Assert.NotNull(v);
            Assert.NotNull(v.Do2());
            Assert.NotNull(v.Do());
            Assert.Equal(3, v.Do3());
            Assert.Equal(13, v.Do4());
            Assert.Equal(20, v.CallDo5());
        }

        [Fact]
        public void AbstractClassWhenHasBaseMethod()
        {
            var proxyType = creator.CreateProxyType(typeof(AbstractMethodTestClass), ProxyTypes.Inherit);
            Assert.Equal("AbstractMethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType) as AbstractMethodTestClass;
            Assert.NotNull(v);
            Assert.NotNull(v.Do2());
            Assert.Null(v.Do());
            Assert.Equal(3, v.Do3());
            Assert.Equal(13, v.Do4());
        }

        [Fact]
        public void InterfaceWhenHasDefaultMethod()
        {
            var proxyType = creator.CreateProxyType(typeof(IMethodTestInterface), ProxyTypes.Inherit);
            Assert.Equal("IMethodTestInterface_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType) as IMethodTestInterface;
            Assert.NotNull(v);
            Assert.NotNull(v.Do2());
            Assert.Null(v.Do());
        }

        [Fact]
        public void InterfaceWhenVirtualMethod()
        {
            var proxyType = creator.CreateProxyType(typeof(IMethodTestInterface), ProxyTypes.Facade);
            Assert.Equal("IMethodTestInterface_Proxy_Facade", proxyType.Name);
            var v = Activator.CreateInstance(proxyType) as IMethodTestInterface;
            Assert.NotNull(v);
            var f = proxyType.GetField("instance", BindingFlags.NonPublic | BindingFlags.Instance);
            var m = new Mock<IMethodTestInterface>();
            f.SetValue(v, m.Object);
            Assert.Null(v.Do());
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
        public void WhenNotVirtualNoArgsReturnInt()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType) as MethodTestClass;
            Assert.NotNull(v);
            Assert.Equal(3, v.NotVirtualNoArgsReturnInt());
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

        [Fact]
        public void WhenHasArgsReturnTuple()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType) as MethodTestClass;
            Assert.NotNull(v);
            long i = 9;
            Assert.Equal((7, 9), v.HasArgsReturnTuple(7, ref i));
        }

        [Fact]
        public void WhenHasArgsReturnArray()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType) as MethodTestClass;
            Assert.NotNull(v);
            var array = v.HasArgsReturnArray(7, out var i);
            Assert.Equal(2, array.Length);
            Assert.Equal(7, array[0]);
            Assert.Equal(99L, array[1]);
            Assert.Equal(99L, i);
        }
    }
}