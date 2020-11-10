using System;

namespace Norns.Urd.Proxy
{
    public interface IProxyGenerator
    {
        ProxyTypes ProxyType { get; }

        string GetProxyTypeName(Type serviceType);

        Type CreateProxyType(ProxyGeneratorContext context);
    }
}