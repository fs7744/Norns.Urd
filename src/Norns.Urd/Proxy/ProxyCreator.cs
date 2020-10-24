using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.Proxy
{
    public class ProxyCreator : IProxyCreator
    {
        const string GeneratorAssemblyName = "Norns.Urd.DynamicGenerator";
        private readonly ModuleBuilder moduleBuilder;
        private readonly Dictionary<ProxyTypes, IProxyGenerator> generators;
        private readonly Dictionary<string, Type> definedTypes = new Dictionary<string, Type>();
        private readonly object _lock = new object();

        public ProxyCreator(IEnumerable<IProxyGenerator> generators)
        {
            this.generators = generators.ToDictionary(i => i.ProxyType);
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(GeneratorAssemblyName), AssemblyBuilderAccess.RunAndCollect);
            moduleBuilder = asmBuilder.DefineDynamicModule("core");
        }

        public Type CreateProxyType(Type serviceType, ProxyTypes proxyType = ProxyTypes.Inherit)
        {
            if(!generators.TryGetValue(proxyType, out var generator)) return null;
            lock (_lock)
            {
                var name = generator.GetProxyTypeName(serviceType);
                if (!definedTypes.TryGetValue(name, out Type type))
                {
                    type = generator.CreateProxyType(new ProxyGeneratorContext()
                    {
                        ProxyTypeName = name,
                        ServiceType = serviceType,
                        ModuleBuilder = moduleBuilder
                    });
                    definedTypes[name] = type;
                }
                return type;
            }
        }
    }
}