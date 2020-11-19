using System.Reflection;
using System.Threading.Tasks;

namespace Norns.Urd
{
    public interface IInterceptor
    {
        int Order { get; }

        void Invoke(AspectContext context, AspectDelegate next);

        Task InvokeAsync(AspectContext context, AsyncAspectDelegate next);

        bool CanAspect(MethodInfo method);
    }
}