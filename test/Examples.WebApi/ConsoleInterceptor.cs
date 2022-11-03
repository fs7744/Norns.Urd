using Examples.WebApi.Controllers;
using Norns.Urd;
using Norns.Urd.Reflection;
using System;
using System.Threading.Tasks;

namespace Examples.WebApi
{
    public class ConsoleInterceptor : AbstractInterceptor
    {
        public override void Invoke(AspectContext context, AspectDelegate next)
        {
            context.ServiceProvider.GetService(typeof(IAopTest));
            Console.WriteLine($"{context.Service.GetType().GetReflector().FullDisplayName}.{context.Method.GetReflector().DisplayName}");
            next(context);
        }

        public override async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            Console.WriteLine($"{context.Service.GetType().GetReflector().FullDisplayName}.{context.Method.GetReflector().DisplayName}");
            await next(context);
        }
    }
}