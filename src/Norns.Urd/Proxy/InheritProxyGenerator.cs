namespace Norns.Urd.Proxy
{
    public class InheritProxyGenerator : FacadeProxyGenerator
    {
        public InheritProxyGenerator(IInterceptorFactory interceptorFactory) : base(interceptorFactory)
        {
        }

        public override ProxyTypes ProxyType { get; } = ProxyTypes.Inherit;
    }
}