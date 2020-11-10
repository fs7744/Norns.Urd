using System.Threading.Tasks;

namespace Norns.Urd
{
    public interface IInterceptor
    {
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

        Task InvokeAsync(AspectContext context, AspectDelegateAsync next);
    }
}