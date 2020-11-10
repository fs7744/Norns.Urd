using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace Norns.Urd.IOC
{
    public interface IProxyServiceDescriptorConverter
    {
        ServiceDescriptor Convert(ServiceDescriptor descriptor);
    }

    public class ProxyServiceDescriptorConverter : IProxyServiceDescriptorConverter
    {
        private readonly IServiceDescriptorConvertHandler[] handlers;

        public ProxyServiceDescriptorConverter(IEnumerable<IServiceDescriptorConvertHandler> handlers)
        {
            this.handlers = handlers.ToArray();
        }

        public ServiceDescriptor Convert(ServiceDescriptor descriptor)
        {
            return handlers.FirstOrDefault(i => i.CanConvert(descriptor))?
                .Convert(descriptor);
        }
    }
}