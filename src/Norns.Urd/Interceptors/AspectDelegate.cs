using System.Threading.Tasks;

namespace Norns.Urd
{
    public delegate Task AsyncAspectDelegate(AspectContext context);

    public delegate void AspectDelegate(AspectContext context);

    internal delegate Task CallAsyncAspectDelegate(AspectContext context, AsyncAspectDelegate next);

    internal delegate void CallAspectDelegate(AspectContext context, AspectDelegate next);
}