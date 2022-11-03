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

    public class ProxyGenerator : IProxyGenerator
    {
        private readonly ModuleBuilder moduleBuilder;
        private readonly IInterceptorCreator interceptorCreator;
        private readonly FacadeProxyBuilder facade = new FacadeProxyBuilder();
        private readonly InheritProxyBuilder inherit = new InheritProxyBuilder();

        public ProxyGenerator(IAspectConfiguration configuration)
        {
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Constants.GeneratedNamespace), AssemblyBuilderAccess.RunAndCollect);
            moduleBuilder = asmBuilder.DefineDynamicModule("core");
            interceptorCreator = new InterceptorCreator(configuration);
        }

        public Type Create(Type serviceType, ProxyTypes proxyType)
        {
            switch (proxyType)
            {
                case ProxyTypes.Inherit:
                    return inherit.Create(serviceType, interceptorCreator, moduleBuilder);

                case ProxyTypes.Facade:
                    return facade.Create(serviceType, interceptorCreator, moduleBuilder);

                default:
                    return null;
            }
        }
    }
}