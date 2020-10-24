using Microsoft.CodeAnalysis;
using Norns.Destiny.AOP;
using Norns.Skuld.AOP;
using System.Collections.Generic;

namespace Norns.DestinyLoom.Test
{
    [Generator]
    public class ProxyGenerator : AopSourceGenerator
    {
        protected override IEnumerable<IInterceptorGenerator> GetInterceptorGenerators()
        {
            yield return new ConsoleCallMethodGenerator();
        }
    }
}