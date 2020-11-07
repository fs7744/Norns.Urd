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
            if (context.ReturnValue != null)
            {
                context.ReturnValue = (int)context.ReturnValue + 10;
            }
        }
    }

    public class InheritInterceptorFactoryMethodTestClass : InterceptorFactoryMethodTestClass
    {
        public int BaseNoArgsReturnInt()
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

        public AspectDelegate CallMethod(MethodInfo method)
        {
            AspectDelegate action = c => c.ReturnValue = method.Invoke(c.Service, c.Parameters);
            return action;
        }

        [Fact]
        public void SyncWhenPublicMethod()
        {
            var m = typeof(InterceptorFactoryMethodTestClass).GetMethod(nameof(InterceptorFactoryMethodTestClass.NoArgsVoid));
            interceptorFactory.CreateInterceptor(m, CallMethod(m));
            var context = new AspectContext(m, new InterceptorFactoryMethodTestClass(), ProxyTypes.Facade);
            interceptorFactory.CallInterceptor(context);
            Assert.Null(context.ReturnValue);
        }

        [Fact]
        public void SyncWhenPublicMethodReturnInt()
        {
            var m = typeof(InterceptorFactoryMethodTestClass).GetMethod(nameof(InterceptorFactoryMethodTestClass.NoArgsReturnInt));
            interceptorFactory.CreateInterceptor(m, CallMethod(m));
            var context = new AspectContext(m, new InterceptorFactoryMethodTestClass(), ProxyTypes.Facade);
            interceptorFactory.CallInterceptor(context);
            Assert.Equal(23, (int)context.ReturnValue);
        }

        [Fact]
        public void InheritSyncWhenPublicMethodReturnInt()
        {
            var m = typeof(InterceptorFactoryMethodTestClass).GetMethod(nameof(InterceptorFactoryMethodTestClass.NoArgsReturnInt));
            interceptorFactory.CreateInterceptor(m, c => c.ReturnValue = ((InheritInterceptorFactoryMethodTestClass)c.Service).BaseNoArgsReturnInt(), ProxyTypes.Inherit);
            var context = new AspectContext(m, new InheritInterceptorFactoryMethodTestClass(), ProxyTypes.Inherit);
            interceptorFactory.CallInterceptor(context);
            Assert.Equal(23, (int)context.ReturnValue);
        }

        [Fact]
        public void SyncWhenPublicMethodHasOneArgsReturnInt()
        {
            var m = typeof(InterceptorFactoryMethodTestClass).GetMethod(nameof(InterceptorFactoryMethodTestClass.HasOneArgsReturnInt));
            interceptorFactory.CreateInterceptor(m, CallMethod(m));
            var context = new AspectContext(m, new InterceptorFactoryMethodTestClass(), ProxyTypes.Facade)
            {
                Parameters = new object[] { 4 }
            };
            interceptorFactory.CallInterceptor(context);
            Assert.Equal(27, (int)context.ReturnValue);
        }
    }
}