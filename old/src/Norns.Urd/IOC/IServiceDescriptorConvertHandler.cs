using Microsoft.Extensions.DependencyInjection;

namespace Norns.Urd.IOC
{
    public interface IServiceDescriptorConvertHandler
    {
        bool CanConvert(ServiceDescriptor descriptor);

        ServiceDescriptor Convert(ServiceDescriptor descriptor);
    }
}