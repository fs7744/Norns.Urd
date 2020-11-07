using Norns.Urd.Proxy;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Norns.Urd.UT
{
    public class InterceptorFactoryMethodTestClass
    {
        public virtual void NoArgsVoid()
        {
        }

        public virtual int NoArgsReturnInt() => 3;

        public virtual int HasOneArgsReturnInt(int num) => num + 3;
    }

    public class TestInterceptor : IInterceptor
    {
        public async Task InvokeAsync(AspectContext context, AspectDelegateAsync next)
        {
            await next(context);
            if (context.ReturnValue is int y)
            {
                context.ReturnValue = y + 10;
            }
        }
    }

    public class InheritInterceptorFactoryMethodTestClass : InterceptorFactoryMethodTestClass
    {
        public int NoArgsReturnInt_Base()
        {
            return base.NoArgsReturnInt();
        }

        public override int NoArgsReturnInt()
        {
            return base.NoArgsReturnInt() + 3;
        }
    }

    public class InterceptorFactoryTest
    {
        private readonly InterceptorFactory interceptorFactory;

        public InterceptorFactoryTest()
        {
            var config = new AspectConfiguration();
            config.Interceptors.Add(new TestInterceptor());
            config.Interceptors.Add(new TestInterceptor());
            interceptorFactory = new InterceptorFactory(config);
        }

        [Fact]
        public void SyncWhenPublicMethod()
        {
            var m = typeof(InterceptorFactoryMethodTestClass).GetMethod(nameof(InterceptorFactoryMethodTestClass.NoArgsVoid));
            var context = new AspectContext(m, new InterceptorFactoryMethodTestClass(), ProxyTypes.Facade, new object[0]);
            interceptorFactory.GetInterceptor(m, ProxyTypes.Facade)(context);
            Assert.Null(context.ReturnValue);
        }

        [Fact]
        public void SyncWhenPublicMethodReturnInt()
        {
            var m = typeof(InterceptorFactoryMethodTestClass).GetMethod(nameof(InterceptorFactoryMethodTestClass.NoArgsReturnInt));
            var context = new AspectContext(m, new InterceptorFactoryMethodTestClass(), ProxyTypes.Facade, new object[0]);
            interceptorFactory.GetInterceptor(m, ProxyTypes.Facade)(context);
            Assert.Equal(23, (int)context.ReturnValue);
        }

        [Fact]
        public void InheritSyncWhenPublicMethodReturnInt()
        {
            var m = typeof(InterceptorFactoryMethodTestClass).GetMethod(nameof(InterceptorFactoryMethodTestClass.NoArgsReturnInt));
            var context = new AspectContext(m, new InheritInterceptorFactoryMethodTestClass(), ProxyTypes.Inherit, new object[0]);
            interceptorFactory.GetInterceptor(m, ProxyTypes.Inherit)(context);
            Assert.Equal(23, (int)context.ReturnValue);
        }

        [Fact]
        public void SyncWhenPublicMethodHasOneArgsReturnInt()
        {
            var m = typeof(InterceptorFactoryMethodTestClass).GetMethod(nameof(InterceptorFactoryMethodTestClass.HasOneArgsReturnInt));
            var context = new AspectContext(m, new InterceptorFactoryMethodTestClass(), ProxyTypes.Facade, new object[] { 4 });
            interceptorFactory.GetInterceptor(m, ProxyTypes.Facade)(context);
            Assert.Equal(27, (int)context.ReturnValue);
        }
    }
}