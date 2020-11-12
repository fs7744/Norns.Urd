using Norns.Urd.Interceptors;
using System;
using System.Reflection.Emit;

namespace Norns.Urd.DynamicProxy
{
    public abstract class ProxyBuilderBase : IProxyBuilder
    {
        protected abstract ProxyTypes ProxyType { get; }

        public Type Create(Type serviceType, IInterceptorConfiguration configuration, ModuleBuilder moduleBuilder)
        {
            var context = new ProxyGeneratorContext(moduleBuilder, serviceType, configuration, ProxyType);
            return context.Complete();
        }
    }

    public class FacadeProxyBuilder : ProxyBuilderBase
    {
        protected override ProxyTypes ProxyType { get; } = ProxyTypes.Facade;
    }

    public class InheritProxyBuilder : ProxyBuilderBase
    {
        protected override ProxyTypes ProxyType { get; } = ProxyTypes.Inherit;
    }
}