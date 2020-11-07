using Norns.Urd.Utils;
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
        private readonly IAspectConfiguration configuration;
        private readonly IServiceProvider provider;
        private readonly ConstantInfo constantInfo;

        public ProxyCreator(IEnumerable<IProxyGenerator> generators, IAspectConfiguration configuration, IServiceProvider provider, ConstantInfo constantInfo)
        {
            this.generators = generators.ToDictionary(i => i.ProxyType);
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(GeneratorAssemblyName), AssemblyBuilderAccess.RunAndCollect);
            moduleBuilder = asmBuilder.DefineDynamicModule("core");
            this.configuration = configuration;
            this.provider = provider;
            this.constantInfo = constantInfo;
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
                        ModuleBuilder = moduleBuilder,
                        Configuration = configuration,
                        ServiceProvider = provider,
                        ConstantInfo = constantInfo
                    });
                    definedTypes[name] = type;
                }
                return type;
            }
        }
    }
}