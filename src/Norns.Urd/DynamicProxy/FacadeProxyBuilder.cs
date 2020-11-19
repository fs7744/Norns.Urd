using Norns.Urd.Reflection;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.DynamicProxy
{
    public class FacadeProxyBuilder : ProxyBuilderBase
    {
        protected override ProxyTypes ProxyType { get; } = ProxyTypes.Facade;

        public override void DefineFields(in ProxyGeneratorContext context)
        {
            base.DefineFields(context);
            context.ProxyType.Fields.Add(Constants.Instance, context.ProxyType.TypeBuilder.DefineField(Constants.Instance, context.ServiceType, FieldAttributes.Private));
        }

        protected override FieldBuilder DefineMethodInfoCaller(in ProxyGeneratorContext context, MethodInfo method)
        {
            return context.AssistType.DefineMethodInfoCaller(method, method.GetReflector().DisplayName);
        }

        protected override MethodBuilder DefineNonAspectMethod(in ProxyGeneratorContext context, MethodInfo method)
        {
            var parameters = method.GetParameters().Select(i => i.ParameterType).ToArray();
            MethodBuilder methodBuilder = context.ProxyType.TypeBuilder.DefineMethod(method.Name, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, parameters);
            methodBuilder.DefineGenericParameter(method);
            methodBuilder.DefineParameters(method);
            methodBuilder.DefineCustomAttributes(method);
            var il = methodBuilder.GetILGenerator();
            il.EmitThis();
            il.Emit(OpCodes.Ldfld, context.ProxyType.Fields[Constants.Instance]);
            for (var i = 1; i <= parameters.Length; i++)
            {
                il.EmitLoadArg(i);
            }
            il.Emit(OpCodes.Callvirt, method);
            il.Emit(OpCodes.Ret);
            context.ProxyType.TypeBuilder.DefineMethodOverride(methodBuilder, method);
            return methodBuilder;
        }

        protected override void GetServiceInstance(in ProxyGeneratorContext context, ILGenerator il)
        {
            il.EmitThis();
            il.Emit(OpCodes.Ldfld, context.ProxyType.Fields[Constants.Instance]);
        }
    }
}