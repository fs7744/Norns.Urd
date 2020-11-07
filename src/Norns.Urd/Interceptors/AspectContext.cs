using Norns.Urd.Proxy;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Norns.Urd
{
    public class AspectContext
    {
        public AspectContext(MethodInfo serviceMethod, object service, ProxyTypes proxyType)
        {
            ServiceMethod = serviceMethod;
            Service = service;
            ProxyType = proxyType;
        }

        public object ReturnValue { get; set; }
        public IDictionary<string, object> AdditionalData { get; } = new Dictionary<string, object>();
        public IServiceProvider ServiceProvider { get; set; }
        public MethodInfo ServiceMethod { get; }
        public object Service { get; }
        public object[] Parameters { get; set; }
        public ProxyTypes ProxyType { get; }
    }
}