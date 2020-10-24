using System;

namespace Norns.Urd.Proxy
{
    public interface IProxyCreator
    {
        Type CreateProxyType(Type serviceType, ProxyTypes proxyType = ProxyTypes.Inherit);
    }
}