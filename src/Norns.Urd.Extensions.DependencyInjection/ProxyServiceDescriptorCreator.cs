using Norns.Urd.DynamicProxy;
using System;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    internal class ProxyServiceDescriptorCreator
    {
        private readonly IProxyGenerator proxyGenerator;

        public ProxyServiceDescriptorCreator(IProxyGenerator proxyGenerator)
        {
            this.proxyGenerator = proxyGenerator;
        }

        internal bool TryCreate(ServiceDescriptor descriptor, out ServiceDescriptor proxyServiceDescriptor)
        {
            proxyServiceDescriptor = null;
            return descriptor switch
            {
                { ServiceType: { IsSealed: true } }
                or { ServiceType: { IsValueType: true } }
                or { ServiceType: { IsEnum: true } }
                    //|| !serviceType.GetTypeInfo().IsVisible()
                    => false,
                var i when i.ImplementationFactory is not null => TryCreateFacadeImplementation(i, i.ImplementationFactory, out proxyServiceDescriptor),
                var i when i.ImplementationInstance is not null => TryCreateFacadeImplementation(i, x => i.ImplementationInstance, out proxyServiceDescriptor),
                var i when i.ImplementationType.IsSealed
                    //|| !i.ImplementationType.GetTypeInfo().IsVisible()
                    => TryCreateFacadeImplementation(i, x => ActivatorUtilities.CreateInstance(x, i.ImplementationType), out proxyServiceDescriptor),
                _ => TryCreateInheritImplementation(descriptor, out proxyServiceDescriptor)
            };
        }

        internal bool TryCreateFacadeImplementation(ServiceDescriptor descriptor, Func<IServiceProvider, object> implementationFactory, out ServiceDescriptor proxyServiceDescriptor)
        {
            ServiceDescriptor createServiceDescriptor(ServiceDescriptor x, Type type) => ServiceDescriptor.Describe(x.ServiceType, i =>
            {
                var proxy = ActivatorUtilities.CreateInstance(i, type);
                var f = proxy.GetType().GetField(Constants.Instance, BindingFlags.NonPublic | BindingFlags.Instance); // todo: 性能优化
                f.SetValue(proxy, implementationFactory(i));
                return proxy;
            }, x.Lifetime);

            return TryCreateImplementation(ProxyTypes.Facade, descriptor, createServiceDescriptor, out proxyServiceDescriptor);
        }

        internal bool TryCreateImplementation(ProxyTypes proxyType, ServiceDescriptor descriptor, Func<ServiceDescriptor, Type, ServiceDescriptor> createServiceDescriptor, out ServiceDescriptor proxyServiceDescriptor)
        {
            var implementationType = proxyGenerator.Create(descriptor.ServiceType, proxyType);
            proxyServiceDescriptor = implementationType == null ? null : createServiceDescriptor(descriptor, implementationType);
            return proxyServiceDescriptor != null;
        }

        internal bool TryCreateInheritImplementation(ServiceDescriptor descriptor, out ServiceDescriptor proxyServiceDescriptor)
        {
            static ServiceDescriptor createServiceDescriptor(ServiceDescriptor x, Type type) => ServiceDescriptor.Describe(x.ServiceType, type, x.Lifetime);

            return TryCreateImplementation(ProxyTypes.Inherit, descriptor, createServiceDescriptor, out proxyServiceDescriptor);
        }
    }
}