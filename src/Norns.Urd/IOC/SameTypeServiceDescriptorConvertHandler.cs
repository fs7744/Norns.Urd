using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Proxy;

namespace Norns.Urd.IOC
{
    public class SameTypeServiceDescriptorConvertHandler : IServiceDescriptorConvertHandler
    {
        private readonly IProxyCreator creator;

        public SameTypeServiceDescriptorConvertHandler(IProxyCreator creator)
        {
            this.creator = creator;
        }

        public bool CanConvert(ServiceDescriptor descriptor)
        {
            return descriptor.ImplementationType == descriptor.ServiceType;
        }

        public ServiceDescriptor Convert(ServiceDescriptor descriptor)
        {
            var serviceType = descriptor.ServiceType;
            return ServiceDescriptor.Describe(serviceType, creator.CreateProxyType(serviceType), descriptor.Lifetime);
        }
    }
}