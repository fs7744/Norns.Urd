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
            if (descriptor.ImplementationFactory != null)
            {
                return TryCreateFacadeImplementation(descriptor, descriptor.ImplementationFactory, out proxyServiceDescriptor);
            }
            else if (descriptor.ImplementationInstance != null)
            {
                var instance = descriptor.ImplementationInstance;
                return TryCreateFacadeImplementation(descriptor, x => instance, out proxyServiceDescriptor);
            }
            else if (!descriptor.ServiceType.IsGenericTypeDefinition
                    && (descriptor.ImplementationType.IsSealed
                        || !descriptor.ImplementationType.GetTypeInfo().IsVisible()))
            {
                var type = descriptor.ImplementationType;
                return TryCreateFacadeImplementation(descriptor, x => ActivatorUtilities.CreateInstance(x, type), out proxyServiceDescriptor);
            }
            else
            {
                return TryCreateInheritImplementation(descriptor, out proxyServiceDescriptor);
            }
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

        private static ServiceDescriptor createServiceDescriptor(ServiceDescriptor x, Type type) => ServiceDescriptor.Describe(x.ServiceType, type, x.Lifetime);

        internal bool TryCreateInheritImplementation(ServiceDescriptor descriptor, out ServiceDescriptor proxyServiceDescriptor)
        {
            return TryCreateImplementation(ProxyTypes.Inherit, descriptor.ImplementationType, descriptor, createServiceDescriptor, out proxyServiceDescriptor);
        }
    }
}