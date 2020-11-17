using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Norns.Urd.Test.DependencyInjection
{
    public class AddTenInterceptor : IInterceptor
    {
        public void Invoke(AspectContext context, AspectDelegate next)
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
        }

        public async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            await next(context);
            AddTen(context);
        }
    }

    public static class AopTestExtensions
    {
        public static IServiceProvider ConfigServiceCollectionWithAop(Func<IServiceCollection, IServiceCollection> config)
        {
            return config(new ServiceCollection())
            .ConfigureAop(i => i.GlobalInterceptors.Add(new AddTenInterceptor()))
            .BuildServiceProvider();
        }
    }
}