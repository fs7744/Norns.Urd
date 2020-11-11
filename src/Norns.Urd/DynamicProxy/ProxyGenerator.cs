using Norns.Urd.Interceptors;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.DynamicProxy
{
    public interface IProxyGenerator
    {
        Type Create(Type serviceType, ProxyTypes proxyType);
    }

    public interface IProxyBuilder
    {
        Type Create(Type serviceType, IInterceptorConfiguration configuration, ModuleBuilder moduleBuilder);
    }

    public class ProxyGenerator : IProxyGenerator
    {
        private readonly ModuleBuilder moduleBuilder;
        private readonly IInterceptorConfiguration configuration;
        private readonly FacadeProxyBuilder facade = new FacadeProxyBuilder();
        private readonly InheritProxyBuilder inherit = new InheritProxyBuilder();

        public ProxyGenerator(IInterceptorConfiguration configuration)
        {
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Constants.GeneratedNamespace), AssemblyBuilderAccess.RunAndCollect);
            moduleBuilder = asmBuilder.DefineDynamicModule("core");
            this.configuration = configuration;
        }

        public Type Create(Type serviceType, ProxyTypes proxyType)
        {
            switch (proxyType)
            {
                case ProxyTypes.Inherit:
                    return inherit.Create(serviceType, configuration, moduleBuilder);

                case ProxyTypes.Facade:
                    return facade.Create(serviceType, configuration, moduleBuilder);

                default:
                    return null;
            }
        }
    }
}