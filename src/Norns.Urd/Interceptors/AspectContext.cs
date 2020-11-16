using System;
using System.Collections.Generic;
using System.Reflection;

namespace Norns.Urd
{
    public class AspectContext
    {
        public AspectContext(MethodInfo method, object service, object[] parameters, IServiceProvider serviceProvider)
        {
            AdditionalData = new Dictionary<string, object>();
            ServiceProvider = serviceProvider;
            Method = method;
            Service = service;
            Parameters = parameters;
        }

        public IDictionary<string, object> AdditionalData { get; }
        public IServiceProvider ServiceProvider { get; }
        public MethodInfo Method { get; }
        public object Service { get; }
        public object[] Parameters { get; set; }
        public object ReturnValue { get; set; }
    }
}