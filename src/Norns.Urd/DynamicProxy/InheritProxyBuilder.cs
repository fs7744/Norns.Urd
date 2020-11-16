using Norns.Urd.Reflection;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.DynamicProxy
{
    public class InheritProxyBuilder : ProxyBuilderBase
    {
        protected override ProxyTypes ProxyType { get; } = ProxyTypes.Inherit;

        protected override FieldBuilder DefineMethodInfoCaller(in ProxyGeneratorContext context, MethodInfo method)
        {
            var baseMethodName = $"{method.GetReflector().DisplayName}_Base";
            var parameters = method.GetParameters().Select(i => i.ParameterType).ToArray();
            MethodBuilder methodBaseBuilder = context.ProxyType.TypeBuilder.DefineMethod(baseMethodName, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, parameters);
            methodBaseBuilder.DefineGenericParameter(method);
            var il = methodBaseBuilder.GetILGenerator();
            if (method.IsAbstract)
            {
                il.EmitDefault(method.ReturnType);
            }
            else
            {
                il.EmitThis();
                for (var i = 1; i <= parameters.Length; i++)
                {
                    il.EmitLoadArg(i);
                }
                il.Emit(OpCodes.Call, method);
            }

            il.Emit(OpCodes.Ret);
            return context.AssistType.DefineMethodInfoCaller(methodBaseBuilder, baseMethodName);
        }

        protected override MethodBuilder DefineNonAspectMethod(in ProxyGeneratorContext context, MethodInfo method)
        {
            var parameters = method.GetParameters().Select(i => i.ParameterType).ToArray();
            MethodBuilder methodBuilder = context.ProxyType.TypeBuilder.DefineMethod(method.Name, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, parameters);
            methodBuilder.DefineCustomAttributes(method);
            methodBuilder.DefineGenericParameter(method);
            var il = methodBuilder.GetILGenerator();
            if (method.IsAbstract)
            {
                il.EmitDefault(method.ReturnType);
            }
            else
            {
                il.EmitThis();
                for (var i = 1; i <= parameters.Length; i++)
                {
                    il.EmitLoadArg(i);
                }
                il.Emit(OpCodes.Call, method);
            }

            il.Emit(OpCodes.Ret);
            context.ProxyType.TypeBuilder.DefineMethodOverride(methodBuilder, method);
            return methodBuilder;
        }

        protected override void GetServiceInstance(in ProxyGeneratorContext context, ILGenerator il)
        {
            il.EmitThis();
        }
    }
}