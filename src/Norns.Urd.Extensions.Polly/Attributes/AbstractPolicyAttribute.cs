using Norns.Urd.Reflection;
using Polly;
using System;

namespace Norns.Urd.Extensions.Polly
{
    public abstract class AbstractPolicyAttribute : Attribute, IPolicyBuilder
    {
        public abstract ISyncPolicy Build(MethodReflector method);

        public abstract IAsyncPolicy BuildAsync(MethodReflector method);
    }
}