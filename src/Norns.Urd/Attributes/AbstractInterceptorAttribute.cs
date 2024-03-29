﻿using Norns.Urd.Reflection;
using System;
using System.Threading.Tasks;

namespace Norns.Urd
{
    public abstract class AbstractInterceptorAttribute : Attribute, IInterceptor
    {
        public virtual int Order { get; set; }

        public virtual bool CanAspect(MethodReflector method) => true;

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