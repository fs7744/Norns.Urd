using System.Threading.Tasks;

namespace Norns.Urd
{
    public delegate Task AspectDelegateAsync(AspectContext context);

    public delegate void AspectDelegate(AspectContext context);
}