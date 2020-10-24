using System;
using System.Collections.Generic;
using System.Reflection;

namespace Norns.Urd
{
    public abstract class AspectContext
    {
        public abstract object ReturnValue { get; set; }
        public abstract IDictionary<string, object> AdditionalData { get; }
        public abstract IServiceProvider ServiceProvider { get; }
        public abstract MethodInfo ServiceMethod { get; }
        public abstract object Implementation { get; }
        public abstract MethodInfo ImplementationMethod { get; }
        public abstract object[] Parameters { get; }
        public abstract MethodInfo ProxyMethod { get; }
        public abstract object Proxy { get; }
    }
}