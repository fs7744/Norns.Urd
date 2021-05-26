using Norns.Urd.Reflection;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.DynamicProxy
{
    public class InheritProxyBuilder : ProxyBuilderBase
    {
        protected override ProxyTypes ProxyType { get; } = ProxyTypes.Inherit;

        protected override void CallPropertyInjectInConstructor(in ProxyGeneratorContext context, ILGenerator il)
        {
            il.EmitThis();
            il.EmitThis();
            il.Emit(OpCodes.Ldfld, context.ProxyType.Fields[Constants.ServiceProvider]);
            il.Emit(OpCodes.Call, context.ProxyType.PropertyInject.setter);
        }

        protected override FieldBuilder DefineMethodInfoCaller(in ProxyGeneratorContext context, MethodInfo method, MethodInfo serviceMethod)
        {
            var baseMethodName = $"{method.GetReflector().DisplayName}_Base";
            if (!context.ProxyType.Methods.TryGetValue(baseMethodName, out var methodBaseBuilder))
            {
                var parameters = method.GetParameters().Select(i => i.ParameterType).ToArray();
                methodBaseBuilder = context.ProxyType.TypeBuilder.DefineMethod(baseMethodName, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, parameters);
                context.ProxyType.Methods.Add(baseMethodName, methodBaseBuilder);
                methodBaseBuilder.DefineGenericParameter(method);
                methodBaseBuilder.DefineParameters(method);
                methodBaseBuilder.DefineCustomAttributes(method);
                var serviceMethodReflector = method.GetReflector();
                foreach (var customAttributeData in serviceMethod.CustomAttributes
                    .Where(i => !i.AttributeType.IsSubclassOf(typeof(AbstractInterceptorAttribute)) && !serviceMethodReflector.IsDefined(i.AttributeType)))
                {
                    methodBaseBuilder.SetCustomAttribute(customAttributeData.DefineCustomAttribute());
                }
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
            }

            if (method.ContainsGenericParameters && (!method.IsGenericMethodDefinition || method.DeclaringType.IsGenericTypeDefinition))
            {
                return context.AssistType.DefineOpenGenericMethodInfoCaller(methodBaseBuilder, baseMethodName);
            }
            else
            {
                return context.AssistType.DefineMethodInfoCaller(methodBaseBuilder, baseMethodName);
            }
        }

        protected override MethodBuilder DefineNonAspectMethod(in ProxyGeneratorContext context, MethodInfo method)
        {
            var parameters = method.GetParameters().Select(i => i.ParameterType).ToArray();
            MethodBuilder methodBuilder = context.ProxyType.TypeBuilder.DefineMethod(method.Name, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, parameters);
            methodBuilder.DefineGenericParameter(method);
            methodBuilder.DefineParameters(method);
            methodBuilder.DefineCustomAttributes(method);
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