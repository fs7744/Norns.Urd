using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Extensions;
using System;
using System.Reflection;

namespace Norns.Urd.IOC
{
    public class IngoreServiceDescriptorConvertHandler : IServiceDescriptorConvertHandler
    {
        public bool CanConvert(ServiceDescriptor descriptor)
        {
            Type serviceType = descriptor.ServiceType;
            return serviceType.IsSealed
                || serviceType.IsValueType
                || serviceType.IsEnum
                || !serviceType.GetTypeInfo().IsVisible();
        }

        public ServiceDescriptor Convert(ServiceDescriptor descriptor)
        {
            return null;
        }
    }
}