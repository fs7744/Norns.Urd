namespace Norns.Urd.Proxy
{
    public class InheritProxyGenerator : FacadeProxyGenerator
    {
        public override ProxyTypes ProxyType { get; } = ProxyTypes.Inherit;
    }
}