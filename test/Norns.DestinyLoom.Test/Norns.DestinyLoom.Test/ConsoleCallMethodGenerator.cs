using Norns.Destiny.AOP;
using Norns.Destiny.AOP.Notations;
using Norns.Destiny.Notations;
using Norns.Destiny.Structure;
using System.Collections.Generic;
using System.Linq;

namespace Norns.DestinyLoom.Test
{
    public class ConsoleCallMethodGenerator : AbstractInterceptorGenerator
    {
        public override IEnumerable<INotation> BeforeMethod(ProxyGeneratorContext context, IMethodSymbolInfo method)
        {
            if (!method.Parameters.IsEmpty)
            {
                yield return $"System.Console.WriteLine($\"Call Method {method.Name} at {{System.DateTime.Now.ToString(\"yyyy-MM-dd HH:mm:ss.fff\")}} {method.Parameters.First().Type.FullName} {method.Parameters.First().Name} = {{{method.Parameters.First().Name}}}".ToNotation();
                foreach (var item in method.Parameters.Skip(1))
                {
                    yield return $", {item.FullName} {item.Name} = {{{item.Name}}}".ToNotation();
                }
                yield return "\");".ToNotation();
            }
        }

        public override IEnumerable<INotation> AfterMethod(ProxyGeneratorContext context, IMethodSymbolInfo method)
        {
            if (method.HasReturnValue)
            {
                yield return $"System.Console.WriteLine($\"return {{{context.GetReturnValueParameterName()}}} at {{System.DateTime.Now.ToString(\"yyyy-MM-dd HH:mm:ss.fff\")}}\");".ToNotation();
            }
        }
    }
}