using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Proxy;

namespace Norns.Urd.IOC
{
    public class ImplementationTypeServiceDescriptorConvertHandler : IServiceDescriptorConvertHandler
    {
        private readonly IProxyCreator creator;

        public ImplementationTypeServiceDescriptorConvertHandler(IProxyCreator creator)
        {
            this.creator = creator;
        }

        public bool CanConvert(ServiceDescriptor descriptor)
        {
            return descriptor.ImplementationType != null;
        }

        public ServiceDescriptor Convert(ServiceDescriptor descriptor)
        {
            return ServiceDescriptor.Describe(descriptor.ServiceType, creator.CreateProxyType(descriptor.ImplementationType), descriptor.Lifetime);
        }
    }
}