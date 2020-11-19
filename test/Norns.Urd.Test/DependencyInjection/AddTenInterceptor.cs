using Norns.Urd;
using System.Threading.Tasks;

namespace Test.Norns.Urd.DependencyInjection
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
            else if(context.ReturnValue is double d)
            {
                context.ReturnValue = d + 10.0;
            }
        }

        public async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            await next(context);
            AddTen(context);
        }
    }
}