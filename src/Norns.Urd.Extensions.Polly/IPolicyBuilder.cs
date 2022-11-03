using Norns.Urd.Reflection;
using Polly;

namespace Norns.Urd.Extensions.Polly
{
    public interface IPolicyBuilder
    {
        ISyncPolicy Build(MethodReflector method);

        IAsyncPolicy BuildAsync(MethodReflector method);
    }
}