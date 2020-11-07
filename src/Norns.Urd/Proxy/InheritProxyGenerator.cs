using Norns.Urd.Extensions;
using Norns.Urd.Utils;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.Proxy
{
    public class InheritProxyGenerator : FacadeProxyGenerator
    {
        public InheritProxyGenerator() : base()
        {
        }

        public override ProxyTypes ProxyType { get; } = ProxyTypes.Inherit;

        public override void DefineMethod(ProxyGeneratorContext context, MethodInfo method)
        {
            if (!method.IsVisibleAndVirtual()) return;
            var baseMethodName = $"{method.Name}_Base";
            MethodBuilder methodBaseBuilder = context.TypeBuilder.DefineMethod(baseMethodName, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, method.GetParameters().Select(i => i.ParameterType).ToArray());
            var ilGen = methodBaseBuilder.GetILGenerator();
            ilGen.EmitThis();
            ilGen.Emit(OpCodes.Call, method);
            ilGen.Emit(OpCodes.Ret);
            base.DefineMethod(context, method);
        }
    }
}