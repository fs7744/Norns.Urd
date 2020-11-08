using Norns.Urd.Extensions;
using Norns.Urd.Proxy;
using Norns.Urd.Utils;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Norns.Urd
{
    public interface IInterceptorFactory
    {
        AspectDelegate GetInterceptor(MethodInfo method, ProxyTypes proxyType);

        AspectDelegateAsync GetInterceptorAsync(MethodInfo method, ProxyTypes proxyType);
    }

    public class InterceptorFactory : IInterceptorFactory
    {
        private readonly IAspectConfiguration configuration;

        public InterceptorFactory(IAspectConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public AspectDelegate GetInterceptor(MethodInfo method, ProxyTypes proxyType)
        {
            AspectDelegate baseCall;
            if (proxyType == ProxyTypes.Facade)
            {
                baseCall = CreateSyncCaller(method);
            }
            else
            {
                var lazyCaller = new LazyCaller($"{method.Name}_Base");
                baseCall = lazyCaller.Call;
            }

            return configuration.Interceptors.Select(i =>
            {
                MAspectDelegate a = i.Invoke;
                return a;
            }).Aggregate(baseCall, (i, j) => c => j(c, i));
        }

        public class LazyCaller
        {
            public LazyCaller(string method)
            {
                this.method = method;
            }

            private AspectDelegate caller;
            private readonly string method;

            public void Call(AspectContext context)
            {
                if (caller == null)
                {
                    caller = CreateSyncCaller(context.Service.GetType().GetMethod(method));
                }
                caller(context);
            }
        }

        public static AspectDelegate CreateSyncCaller(MethodInfo method)
        {
            DynamicMethod dynamicMethod = new DynamicMethod($"invoker_{Guid.NewGuid()}",
        typeof(void), new Type[] { typeof(AspectContext) }, method.Module, true);
            var il = dynamicMethod.GetILGenerator();
            il.EmitLoadArg(0);
            if (!method.IsVoid())
            {
                il.Emit(OpCodes.Dup);
            }
            il.Emit(OpCodes.Call, ConstantInfo.GetService);
            var parameters = method.GetParameters().Select(i => i.ParameterType).ToArray();
            var argsLocal = il.DeclareLocal(typeof(object[]));
            var frefs = new LocalBuilder[parameters.Length];
            if (parameters.Length > 0)
            {
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Call, ConstantInfo.GetParameters);
                il.Emit(OpCodes.Stloc, argsLocal);
                for (var i = 0; i < parameters.Length; i++)
                {
                    il.Emit(OpCodes.Ldloc, argsLocal);
                    il.EmitInt(i);
                    il.Emit(OpCodes.Ldelem_Ref);
                    if (parameters[i].IsByRef)
                    {
                        var defType = parameters[i].GetElementType();
                        var fref = il.DeclareLocal(defType);
                        frefs[i] = fref;
                        il.EmitConvertFromObject(defType);
                        il.Emit(OpCodes.Stloc, fref);
                        il.Emit(OpCodes.Ldloca, fref);
                    }
                    else
                    {
                        il.EmitConvertFromObject(parameters[i]);
                    }
                }
            }
            il.Emit(OpCodes.Callvirt, method);
            if (!method.IsVoid())
            {
                il.EmitConvertToObject(method.ReturnType);
                il.Emit(OpCodes.Call, ConstantInfo.SetReturnValue);
            }
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].IsByRef)
                {
                    il.Emit(OpCodes.Ldloc, argsLocal);
                    il.EmitInt(i);
                    il.Emit(OpCodes.Ldloc, frefs[i]);
                    il.EmitConvertToObject(frefs[i].LocalType);
                    il.Emit(OpCodes.Stelem_Ref);
                }
            }
            il.Emit(OpCodes.Ret);
            return (AspectDelegate)dynamicMethod.CreateDelegate(typeof(AspectDelegate));
        }

        public AspectDelegateAsync GetInterceptorAsync(MethodInfo method, ProxyTypes proxyType)
        {
            AspectDelegateAsync baseCall;
            if (proxyType == ProxyTypes.Facade)
            {
                var syncbaseCall = CreateSyncCaller(method);
                baseCall = async c =>
                {
                    syncbaseCall(c);

                    switch (c.ReturnValue)
                    {
                        case Task t:
                            await t;
                            break;

                        case ValueTask t:
                            await t;
                            break;

                        default:
                            break;
                    }
                };
            }
            else
            {
                var lazyCaller = new LazyCaller($"{method.Name}_Base");
                baseCall = async c =>
                {
                    lazyCaller.Call(c);

                    switch (c.ReturnValue)
                    {
                        case Task t:
                            await t;
                            break;

                        case ValueTask t:
                            await t;
                            break;

                        default:
                            break;
                    }
                };
            }

            return configuration.Interceptors.Select(i =>
            {
                MAspectDelegateAsync a = i.InvokeAsync;
                return a;
            }).Aggregate(baseCall, (i, j) => c => j(c, i));
        }
    }
}