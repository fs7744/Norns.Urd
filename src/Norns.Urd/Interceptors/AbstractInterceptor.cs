﻿using System.Reflection;
using System.Threading.Tasks;

namespace Norns.Urd
{
    [NonAspect]
    public abstract class AbstractInterceptor : IInterceptor
    {
        public virtual int Order => 0;

        public virtual bool CanAspect(MethodInfo method) => true;

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

        public abstract Task InvokeAsync(AspectContext context, AsyncAspectDelegate next);
    }
}