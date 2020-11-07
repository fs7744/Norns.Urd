using Norns.Urd.Extensions;
using System;
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

        public override void DefineMethods(ProxyGeneratorContext context)
        {
            foreach (var method in context.ServiceType.GetTypeInfo().DeclaredMethods)
            {
                var mf = context.AssistStaticTypeBuilder.DefineMethodInfo(method, ProxyType);

                var baseMethodName = $"{method.Name}_Base";
                MethodBuilder methodBaseBuilder = context.TypeBuilder.DefineMethod(baseMethodName, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, method.GetParameters().Select(i => i.ParameterType).ToArray());
                var ilGen = methodBaseBuilder.GetILGenerator();
                ilGen.EmitThis();
                ilGen.Emit(OpCodes.Call, method);
                ilGen.Emit(OpCodes.Ret);

                MethodBuilder methodBuilder = context.TypeBuilder.DefineMethod(method.Name, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, method.GetParameters().Select(i => i.ParameterType).ToArray());
                var il = methodBuilder.GetILGenerator();
                var caller = context.AssistStaticTypeBuilder.Fields[$"cm_{method.Name}"];
                il.Emit(OpCodes.Ldsfld, caller);
                il.Emit(OpCodes.Ldsfld, mf);
                il.EmitThis();
                il.Emit(OpCodes.Ldc_I4_0);
                var f = typeof(AspectContext).GetConstructors().First();
                il.Emit(OpCodes.Newobj, f);
                if (method.ReturnType != typeof(void)) // todo: async
                {
                    var c = il.DeclareLocal(typeof(AspectContext));
                    il.Emit(OpCodes.Stloc, c);
                    il.Emit(OpCodes.Ldloc, c);
                    il.Emit(OpCodes.Call, typeof(AspectDelegate).GetMethod(nameof(AspectDelegate.Invoke)));
                    il.Emit(OpCodes.Ldloc, c);
                    il.Emit(OpCodes.Call, typeof(AspectContext).GetProperty(nameof(AspectContext.ReturnValue)).GetMethod);
                    il.Emit(OpCodes.Unbox_Any, method.ReturnType); // todo: 处理各种类型转换
                }
                else
                {
                    il.Emit(OpCodes.Call, typeof(AspectDelegate).GetMethod(nameof(AspectDelegate.Invoke)));
                }
                il.Emit(OpCodes.Ret);
            }
        }
    }
}