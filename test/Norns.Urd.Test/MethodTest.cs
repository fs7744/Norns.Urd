using Moq;
using Norns.Urd.Proxy;
using Norns.Urd.Utils;
using System;
using System.Reflection;
using System.Threading.Tasks;
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

        public virtual int IntP { get; set; }
    }

    public class GenericMethodTestClass
    {

        public virtual T GenericMethod<T>(T t)
        {
            return t;
        }
    }

    public class AsyncMethodTestClass
    {
        public virtual Task NoArgsVoidTask() => Task.Delay(20);

        public virtual async ValueTask NoArgsVoidValueTask()
        {
            await Task.Delay(10);
        }

        public virtual async ValueTask<int> NoArgsVoidValueTaskReturnInt()
        {
            await Task.Delay(10);
            return 55;
        }

        public virtual async Task<long> NoArgsVoidTaskReturnLong()
        {
            await Task.Delay(10);
            return 66L;
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

        #region Sync

        //[Fact]
        //public void WhenGenericMethod()
        //{
        //    var proxyType = creator.CreateProxyType(typeof(GenericMethodTestClass));
        //    Assert.Equal("GenericMethodTestClass_Proxy_Inherit", proxyType.Name);
        //    var v = Activator.CreateInstance(proxyType, new object[] { null }) as GenericMethodTestClass;
        //    Assert.NotNull(v);
        //    Assert.Equal(76, v.GenericMethod(66));
        //}

        [Fact]
        public void SubClassWhenHasOverrideMethod()
        {
            var proxyType = creator.CreateProxyType(typeof(SubClass), ProxyTypes.Inherit);
            Assert.Equal("SubClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as SubClass;
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
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as AbstractMethodTestClass;
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
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as IMethodTestInterface;
            Assert.NotNull(v);
            Assert.NotNull(v.Do2());
            Assert.Null(v.Do());
        }

        [Fact]
        public void InterfaceWhenVirtualMethod()
        {
            var proxyType = creator.CreateProxyType(typeof(IMethodTestInterface), ProxyTypes.Facade);
            Assert.Equal("IMethodTestInterface_Proxy_Facade", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as IMethodTestInterface;
            Assert.NotNull(v);
            var f = proxyType.GetField(ConstantInfo.Instance, BindingFlags.NonPublic | BindingFlags.Instance);
            var m = new Mock<IMethodTestInterface>();
            f.SetValue(v, m.Object);
            Assert.Null(v.Do());
        }

        [Fact]
        public void WhenPublicMethod()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as MethodTestClass;
            Assert.NotNull(v);
            v.NoArgsVoid();
        }

        [Fact]
        public void WhenIntPProperty()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as MethodTestClass;
            Assert.NotNull(v);
            v.IntP = 5;
            Assert.Equal(15, v.IntP);
            Assert.Equal(15, v.IntP);
        }

        [Fact]
        public void WhenNotVirtualNoArgsReturnInt()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as MethodTestClass;
            Assert.NotNull(v);
            Assert.Equal(3, v.NotVirtualNoArgsReturnInt());
        }

        [Fact]
        public void WhenSubMethodTestClass()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as MethodTestClass;
            Assert.NotNull(v);
            Assert.Equal(13, v.NoArgsReturnInt());
        }

        [Fact]
        public void WhenHasArgsReturnInt()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as MethodTestClass;
            Assert.NotNull(v);
            Assert.Equal(17, v.HasArgsReturnInt(7));
            Assert.Equal(19, v.HasArgsReturnInt(9));
        }

        [Fact]
        public void WhenHasArgsReturnTuple()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as MethodTestClass;
            Assert.NotNull(v);
            long i = 9;
            Assert.Equal((7, 9), v.HasArgsReturnTuple(7, ref i));
        }

        [Fact]
        public void WhenHasArgsReturnArray()
        {
            var proxyType = creator.CreateProxyType(typeof(MethodTestClass));
            Assert.Equal("MethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as MethodTestClass;
            Assert.NotNull(v);
            var array = v.HasArgsReturnArray(7, out var i);
            Assert.Equal(2, array.Length);
            Assert.Equal(7, array[0]);
            Assert.Equal(99L, array[1]);
            Assert.Equal(99L, i);
        }

        #endregion Sync

        #region Async

        [Fact]
        public async Task WhenNoArgsVoidTaskAsync()
        {
            var proxyType = creator.CreateProxyType(typeof(AsyncMethodTestClass));
            Assert.Equal("AsyncMethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as AsyncMethodTestClass;
            Assert.NotNull(v);
            var task = v.NoArgsVoidTask();
            await task;
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task WhenNoArgsVoidValueTaskAsync()
        {
            var proxyType = creator.CreateProxyType(typeof(AsyncMethodTestClass));
            Assert.Equal("AsyncMethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as AsyncMethodTestClass;
            Assert.NotNull(v);
            var task = v.NoArgsVoidValueTask();
            await task;
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void WhenNoArgsVoidValueTaskReturnInt()
        {
            var proxyType = creator.CreateProxyType(typeof(AsyncMethodTestClass));
            Assert.Equal("AsyncMethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as AsyncMethodTestClass;
            Assert.NotNull(v);
            var task = v.NoArgsVoidValueTaskReturnInt();
            var a = task.GetAwaiter().GetResult();
            Assert.Equal(55, a);
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public async Task WhenNoArgsVoidTaskReturnLong()
        {
            var proxyType = creator.CreateProxyType(typeof(AsyncMethodTestClass));
            Assert.Equal("AsyncMethodTestClass_Proxy_Inherit", proxyType.Name);
            var v = Activator.CreateInstance(proxyType, new object[] { null }) as AsyncMethodTestClass;
            Assert.NotNull(v);
            var task = v.NoArgsVoidTaskReturnLong();
            var a = await task;
            Assert.Equal(66L, a);
            Assert.True(task.IsCompleted);
        }
        #endregion Async
    }
}