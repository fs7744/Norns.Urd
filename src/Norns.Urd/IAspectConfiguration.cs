using Norns.Urd.IOC;

namespace Norns.Urd
{
    public interface IAspectConfiguration
    {
        InterceptorCollection Interceptors { get; }
        InterceptorFilterCollection Filters { get; }
        IProxyServiceDescriptorConverter Converter { get; set; }
        IInterceptorFactory InterceptorFactory { get; set; }
    }
}