using Polly;

namespace Norns.Urd.Extensions.Polly
{
    public interface ILazyPolicyBuilder
    {
        Lazy<ISyncPolicy, AspectContext> LazyBuild();

        Lazy<IAsyncPolicy, AspectContext> LazyBuildAsync();
    }
}