using Norns.Urd;
using Norns.Urd.DynamicProxy;
using Norns.Urd.Reflection;
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
                var i when i.ImplementationFactory is not null => TryCreateFacadeImplementation(i, i.ImplementationFactory, out proxyServiceDescriptor),
                var i when i.ImplementationInstance is not null => TryCreateFacadeImplementation(i, x => i.ImplementationInstance, out proxyServiceDescriptor),
                var i when !i.ServiceType.IsGenericTypeDefinition
                    && (i.ImplementationType.IsSealed
                        || !i.ImplementationType.GetTypeInfo().IsVisible())
                    => TryCreateFacadeImplementation(i, x => ActivatorUtilities.CreateInstance(x, i.ImplementationType), out proxyServiceDescriptor),
                _ => TryCreateInheritImplementation(descriptor, out proxyServiceDescriptor)
            };
        }

        internal bool TryCreateFacadeImplementation(ServiceDescriptor descriptor, Func<IServiceProvider, object> implementationFactory, out ServiceDescriptor proxyServiceDescriptor)
        {
            ServiceDescriptor createServiceDescriptor(ServiceDescriptor x, Type type)
            {
                var setInstance = type.CreateInstanceSetter();
                var factory = type.CreateFacadeInstanceCreator();
                return ServiceDescriptor.Describe(x.ServiceType, i =>
                {
                    var proxy = factory(i);
                    setInstance(proxy, implementationFactory(i));
                    return proxy;
                }, x.Lifetime);
            }

            return TryCreateImplementation(ProxyTypes.Facade, descriptor.ServiceType, descriptor, createServiceDescriptor, out proxyServiceDescriptor);
        }

        internal bool TryCreateImplementation(ProxyTypes proxyType, Type needPeoxyType, ServiceDescriptor descriptor, Func<ServiceDescriptor, Type, ServiceDescriptor> createServiceDescriptor, out ServiceDescriptor proxyServiceDescriptor)
        {
            var implementationType = proxyGenerator.Create(needPeoxyType, proxyType);
            proxyServiceDescriptor = implementationType == null ? null : createServiceDescriptor(descriptor, implementationType);
            return proxyServiceDescriptor != null;
        }

        internal bool TryCreateInheritImplementation(ServiceDescriptor descriptor, out ServiceDescriptor proxyServiceDescriptor)
        {
            static ServiceDescriptor createServiceDescriptor(ServiceDescriptor x, Type type) => ServiceDescriptor.Describe(x.ServiceType, type, x.Lifetime);

            return TryCreateImplementation(ProxyTypes.Inherit, descriptor.ImplementationType, descriptor, createServiceDescriptor, out proxyServiceDescriptor);
        }
    }
}