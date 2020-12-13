using System;
using System.Threading.Tasks;

namespace Norns.Urd
{
    public class FallbackAttribute : AbstractInterceptorAttribute
    {
        private readonly IInterceptor interceptor;
        public override int Order { get; set; } = -90001;

        public FallbackAttribute(Type interceptorType)
        {
            interceptor = Activator.CreateInstance(interceptorType) as IInterceptor;
            if (interceptor == null)
            {
                throw new ArgumentException("InterceptorType must be IInterceptor type.");
            }
        }

        public override void Invoke(AspectContext context, AspectDelegate next)
        {
            try
            {
                next(context);
            }
            catch (Exception ex)
            {
                context.SetException(ex);
                interceptor.Invoke(context, next);
            }
        }

        public override async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                context.SetException(ex);
                await interceptor.InvokeAsync(context, next);
            }
        }
    }
}