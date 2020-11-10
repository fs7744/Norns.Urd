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
            var context = new ProxyGeneratorContext()
            {
                Configuration = configuration,
                ModuleBuilder = moduleBuilder,
                ServiceType = serviceType
            };
            DefineType(context);
            var type = context.TypeBuilder.CreateTypeInfo().AsType();
            var staticType = context.TypeBuilder == context.StaticTypeBuilder
                ? type
                : context.StaticTypeBuilder.CreateTypeInfo().AsType();
            staticType.GetMethod(Constants.Init).Invoke(null, new object[] { configuration });
            return type;
        }

        private void DefineType(ProxyGeneratorContext context)
        {
            throw new NotImplementedException();
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