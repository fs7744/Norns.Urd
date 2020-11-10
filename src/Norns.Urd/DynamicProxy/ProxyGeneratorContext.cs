using Norns.Urd.Interceptors;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Norns.Urd.DynamicProxy
{
    public class ProxyGeneratorContext
    {
        public Type ServiceType { get; set; }

        public ModuleBuilder ModuleBuilder { get; set; }

        public IInterceptorConfiguration Configuration { get; set; }

        public TypeBuilder TypeBuilder { get; set; }

        public Dictionary<string, FieldBuilder> Fields { get; } = new Dictionary<string, FieldBuilder>();

        public TypeBuilder StaticTypeBuilder { get; set; }

        public Dictionary<string, FieldBuilder> StaticFields { get; } = new Dictionary<string, FieldBuilder>();
    }
}