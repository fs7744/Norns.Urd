using System;
using System.Collections.Generic;
using System.Reflection;

namespace Norns.Urd
{
    public class AspectContext
    {
        public object ReturnValue { get; set; }
        public IDictionary<string, object> AdditionalData { get; set; } = new Dictionary<string, object>();
        public IServiceProvider ServiceProvider { get; set; }
        public MethodInfo ServiceMethod { get; set; }
        public object Service { get; set; }
        public object[] Parameters { get; set; }
    }
}