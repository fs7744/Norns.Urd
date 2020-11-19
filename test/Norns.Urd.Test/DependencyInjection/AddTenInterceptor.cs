using Norns.Urd;
using System.Threading.Tasks;
using System;

namespace Test.Norns.Urd.DependencyInjection
{
    public class AddTenInterceptor : AbstractInterceptor
    {
        public override void Invoke(AspectContext context, AspectDelegate next)
        {
            next(context);
            AddTen(context);
        }

        private static void AddTen(AspectContext context)
        {
            if (context.ReturnValue is int i)
            {
                context.ReturnValue = i + 10;
            }
            else if(context.ReturnValue is double d)
            {
                context.ReturnValue = d + 10.0;
            }
        }

        public override async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            await next(context);
            AddTen(context);
        }
    }

    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class AddSixInterceptorAttribute : AbstractInterceptorAttribute
    {
        private static void AddTen(AspectContext context)
        {
            if (context.ReturnValue is int i)
            {
                context.ReturnValue = i + 6;
            }
            else if (context.ReturnValue is double d)
            {
                context.ReturnValue = d + 6;
            }
        }

        public override async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            await next(context);
            AddTen(context);
        }
    }
}