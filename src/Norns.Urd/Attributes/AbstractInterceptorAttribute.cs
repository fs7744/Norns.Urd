using System;
using System.Threading.Tasks;

namespace Norns.Urd
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Interface, Inherited = false)]
    [NonAspect]
    public abstract class AbstractInterceptorAttribute : Attribute, IInterceptor
    {
        public int Order { get; set; }

        public virtual void Invoke(AspectContext context, AspectDelegate next)
        {
            InvokeAsync(context, c =>
            {
                next(c);
                return Task.CompletedTask;
            }).ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public abstract Task InvokeAsync(AspectContext context, AspectDelegateAsync next);
    }
}