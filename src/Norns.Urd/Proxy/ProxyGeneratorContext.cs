using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Norns.Urd.Proxy
{
    public class ProxyGeneratorContext
    {
        public Type ServiceType { get; set; }

        public string ProxyTypeName { get; set; }

        public ModuleBuilder ModuleBuilder { get; set; }

        public TypeBuilder TypeBuilder { get; set; }

        public Dictionary<string, FieldBuilder> Fields { get; set; } = new Dictionary<string, FieldBuilder>();
    }
}
