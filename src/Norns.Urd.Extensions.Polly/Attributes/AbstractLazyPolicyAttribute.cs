using Polly;
using System;

namespace Norns.Urd.Extensions.Polly
{
    public abstract class AbstractLazyPolicyAttribute : Attribute, ILazyPolicyBuilder
    {
        public abstract ISyncPolicy Build(AspectContext context);

        public abstract IAsyncPolicy BuildAsync(AspectContext context);

        public Lazy<ISyncPolicy, AspectContext> LazyBuild()
        {
            return new Lazy<ISyncPolicy, AspectContext>(Build);
        }

        public Lazy<IAsyncPolicy, AspectContext> LazyBuildAsync()
        {
            return new Lazy<IAsyncPolicy, AspectContext>(BuildAsync);
        }
    }
}