using Norns.Urd.IOC;

namespace Norns.Urd
{
    public class AspectConfiguration : IAspectConfiguration
    {
        public InterceptorCollection Interceptors { get; } = new InterceptorCollection();

        public InterceptorFilterCollection Filters { get; } = new InterceptorFilterCollection();

        public IProxyServiceDescriptorConverter Converter { get; set; }

        public IInterceptorFactory InterceptorFactory { get; set; }
    }
}