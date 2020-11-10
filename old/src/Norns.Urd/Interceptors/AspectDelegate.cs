using System.Threading.Tasks;

namespace Norns.Urd
{
    public delegate Task AspectDelegateAsync(AspectContext context);

    public delegate void AspectDelegate(AspectContext context);

    public delegate Task MAspectDelegateAsync(AspectContext context, AspectDelegateAsync next);

    public delegate void MAspectDelegate(AspectContext context, AspectDelegate next);
}