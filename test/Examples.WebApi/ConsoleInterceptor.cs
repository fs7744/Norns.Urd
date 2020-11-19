using Norns.Urd;
using Norns.Urd.Reflection;
using System;
using System.Threading.Tasks;

namespace Examples.WebApi
{
    public class ConsoleInterceptor : IInterceptor
    {
        public void Invoke(AspectContext context, AspectDelegate next)
        {
            Console.WriteLine($"{context.Service.GetType().GetReflector().FullDisplayName}.{context.Method.GetReflector().DisplayName}");
            next(context);
        }

        public async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            Console.WriteLine($"{context.Service.GetType().GetReflector().FullDisplayName}.{context.Method.GetReflector().DisplayName}");
            await next(context);
        }
    }
}