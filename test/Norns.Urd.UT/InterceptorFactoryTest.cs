using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Norns.Urd.UT
{
    public class InterceptorFactoryMethodTestClass
    {
        public virtual void NoArgsVoid()
        {
        }
    }

    public class TestInterceptor : IInterceptor
    {
        public void Invoke(AspectContext context, AspectDelegate next)
        {
            next(context);
        }

        public Task InvokeAsync(AspectContext context, AspectDelegateAsync next)
        {
            throw new NotImplementedException();
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
        public void WhenPublicMethod()
        {
            var m = typeof(InterceptorFactoryMethodTestClass).GetMethod(nameof(InterceptorFactoryMethodTestClass.NoArgsVoid));
            interceptorFactory.CreateInterceptor(m);
            interceptorFactory.CallInterceptor(new AspectContext() { ServiceMethod = m, Service = new InterceptorFactoryMethodTestClass() });
        }
    }
}
