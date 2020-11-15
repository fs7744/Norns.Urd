using Norns.Urd.Attributes;
using Norns.Urd.Interceptors;
using Norns.Urd.Reflection;
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
            if (IsIgnoreType(serviceType, configuration)) return null;
            var context = new ProxyGeneratorContext(moduleBuilder, serviceType, configuration, ProxyType);
            DefineFields(context);
            DefineCustomAttributes(context);
            return context.Complete();
        }

        private bool IsIgnoreType(Type serviceType, IInterceptorConfiguration configuration) => serviceType switch
        {
            { IsSealed: true }
            or { IsValueType: true }
            or { IsEnum: true }
                    => true,
            _ when !serviceType.GetTypeInfo().IsVisible() => true,
            _ => configuration.IsIgnoreType(serviceType)
        };

        private void DefineCustomAttributes(in ProxyGeneratorContext context)
        {
            context.ProxyType.TypeBuilder.SetCustomAttribute(AttributeExtensions.DefineCustomAttribute<NonAspectAttribute>());
            context.ProxyType.TypeBuilder.SetCustomAttribute(AttributeExtensions.DefineCustomAttribute<DynamicProxyAttribute>());
        }

        public virtual void DefineFields(in ProxyGeneratorContext context)
        {
            context.ProxyType.Fields.Add(Constants.ServiceProvider, context.ProxyType.TypeBuilder.DefineField(Constants.ServiceProvider, typeof(IServiceProvider), FieldAttributes.Private));
        }
    }

    public class FacadeProxyBuilder : ProxyBuilderBase
    {
        protected override ProxyTypes ProxyType { get; } = ProxyTypes.Facade;

        public override void DefineFields(in ProxyGeneratorContext context)
        {
            base.DefineFields(context);
            context.ProxyType.Fields.Add(Constants.Instance, context.ProxyType.TypeBuilder.DefineField(Constants.Instance, context.ServiceType, FieldAttributes.Private));
        }
    }

    public class InheritProxyBuilder : ProxyBuilderBase
    {
        protected override ProxyTypes ProxyType { get; } = ProxyTypes.Inherit;
    }
}