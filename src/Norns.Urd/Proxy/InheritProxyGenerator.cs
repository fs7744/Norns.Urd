using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.Proxy
{
    public class InheritProxyGenerator : FacadeProxyGenerator
    {
        public InheritProxyGenerator(IInterceptorFactory interceptorFactory) : base(interceptorFactory)
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
                il.EmitThis();
                il.Emit(OpCodes.Ldfld, context.Fields[InterceptorFactory]);
                il.Emit(OpCodes.Ldsfld, mf);
                il.EmitThis();
                il.Emit(OpCodes.Ldc_I4_0);
                var f = typeof(AspectContext).GetConstructors().First();
                il.Emit(OpCodes.Newobj, f);
                if (method.ReturnType != typeof(void))
                {
                    var c = il.DeclareLocal(typeof(AspectContext));
                    il.Emit(OpCodes.Stloc, c);
                    il.Emit(OpCodes.Ldloc, c);
                    il.Emit(OpCodes.Callvirt, typeof(IInterceptorFactory).GetMethod(nameof(IInterceptorFactory.CallInterceptor)));
                    il.Emit(OpCodes.Ldloc, c);
                    il.Emit(OpCodes.Call, typeof(AspectContext).GetProperty(nameof(AspectContext.ReturnValue)).GetMethod);
                    il.Emit(OpCodes.Unbox_Any, method.ReturnType); // todo: 处理各种类型转换
                }
                else
                {
                    il.Emit(OpCodes.Callvirt, typeof(IInterceptorFactory).GetMethod(nameof(IInterceptorFactory.CallInterceptor)));
                }
                il.Emit(OpCodes.Ret);
                //context.TypeBuilder.DefineMethodOverride(methodBuilder, method);
                interceptorFactory.CreateInterceptor(method, c => c.ReturnValue = c.Service.GetType().GetMethod(baseMethodName).Invoke(c.Service, c.Parameters), ProxyTypes.Inherit);
            }
        }
    }
}