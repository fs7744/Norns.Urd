using Norns.Urd.Interceptors;
using Norns.Urd.Reflection;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.DynamicProxy
{
    public readonly struct ProxyGeneratorContext
    {
        public ProxyGeneratorContext(ModuleBuilder moduleBuilder, Type serviceType, IInterceptorConfiguration configuration, ProxyTypes proxyType)
        {
            ServiceType = serviceType.GetTypeInfo();
            Configuration = configuration;
            ProxyType = new TypeBuilderContainer(moduleBuilder, moduleBuilder.DefineProxyType(ServiceType, proxyType));
            AssistType = new AssistTypeBuilderContainer(moduleBuilder, moduleBuilder.DefineProxyAssistType(ProxyType.TypeBuilder));
        }

        public TypeInfo ServiceType { get; }

        public IInterceptorConfiguration Configuration { get; }

        public TypeBuilderContainer ProxyType { get; }

        public AssistTypeBuilderContainer AssistType { get; }

        public Type Complete()
        {
            var type = ProxyType.Complete();
            AssistType.Complete(Configuration);
            return type;
        }
    }
}