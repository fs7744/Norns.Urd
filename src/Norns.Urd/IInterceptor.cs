using System.Threading.Tasks;

namespace Norns.Urd
{
    public interface IInterceptor
    {
        void Invoke(AspectContext context, AspectDelegate next);

        Task InvokeAsync(AspectContext context, AspectDelegateAsync next);
    }
}