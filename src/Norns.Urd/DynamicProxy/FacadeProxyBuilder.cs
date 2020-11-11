using Norns.Urd.Interceptors;
using System;
using System.Reflection;
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
                ServiceType = serviceType.GetTypeInfo()
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
            //context.TypeBuilder = context.ModuleBuilder.DefineType(
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