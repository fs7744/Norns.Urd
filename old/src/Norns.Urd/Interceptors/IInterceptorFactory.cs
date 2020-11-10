using Norns.Urd.Extensions;
using Norns.Urd.Proxy;
using Norns.Urd.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        AspectDelegate GetGenericInterceptor(MethodInfo method, ProxyTypes proxyType);

        AspectDelegateAsync GetGenericInterceptorAsync(MethodInfo method, ProxyTypes proxyType);
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

            return FindInterceptors(method).Select(i =>
            {
                MAspectDelegate a = i.Invoke;
                return a;
            }).Aggregate(baseCall, (i, j) => c => j(c, i));
        }

        public AspectDelegate GetGenericInterceptor(MethodInfo method, ProxyTypes proxyType)
        {
            AspectDelegate baseCall;
            var lazyCaller = new LazyGenericCaller(proxyType == ProxyTypes.Facade ? method.Name : $"{method.Name}_Base");
            baseCall = lazyCaller.Call;

            return FindInterceptors(method).Select(i =>
            {
                MAspectDelegate a = i.Invoke;
                return a;
            }).Aggregate(baseCall, (i, j) => c => j(c, i));
        }

        public IEnumerable<IInterceptor> FindInterceptors(MethodInfo method)
        {
            return configuration.Filters.CanAspect(method)
                ? configuration.Interceptors
                : new List<IInterceptor>();
        }

        public class LazyGenericCaller
        {
            private MethodInfo caller;
            private readonly string method;
            private readonly ConcurrentDictionary<MethodInfo, AspectDelegate> callers = new ConcurrentDictionary<MethodInfo, AspectDelegate>();

            public LazyGenericCaller(string method)
            {
                this.method = method;
            }

            public void Call(AspectContext context)
            {
                if (caller == null)
                {
                    caller = context.Service.GetType().GetMethod(method);
                }
                callers.GetOrAdd(context.ServiceMethod, serviceMethod =>
                {
                    var m = caller.MakeGenericMethod(serviceMethod.GetGenericArguments());
                    return CreateSyncCaller(m);
                })(context);
            }
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

        public class AsyncLazyCaller
        {
            public AsyncLazyCaller(string method)
            {
                this.method = method;
            }

            private AspectDelegateAsync caller;
            private readonly string method;

            public async Task Call(AspectContext context)
            {
                if (caller == null)
                {
                    caller = CreateAsyncCaller(context.Service.GetType().GetMethod(method));
                }
                await caller(context);
            }
        }

        public class AsyncLazyGenericCaller
        {
            private MethodInfo caller;
            private readonly string method;
            private readonly ConcurrentDictionary<MethodInfo, AspectDelegateAsync> callers = new ConcurrentDictionary<MethodInfo, AspectDelegateAsync>();

            public AsyncLazyGenericCaller(string method)
            {
                this.method = method;
            }

            public async Task Call(AspectContext context)
            {
                if (caller == null)
                {
                    caller = context.Service.GetType().GetMethod(method);
                }
                await callers.GetOrAdd(context.ServiceMethod, serviceMethod =>
                {
                    var m = caller.MakeGenericMethod(serviceMethod.GetGenericArguments());
                    return CreateAsyncCaller(m);
                })(context);
            }
        }

        public static AspectDelegate CreateSyncCaller(MethodInfo method)
        {
            return (AspectDelegate)CreateCaller(method).CreateDelegate(typeof(AspectDelegate));
        }

        public static AspectDelegateAsync CreateAsyncCaller(MethodInfo method)
        {
            return (AspectDelegateAsync)CreateCaller(method).CreateDelegate(typeof(AspectDelegateAsync));
        }

        public static DynamicMethod CreateCaller(MethodInfo method)
        {
            var isAsync = method.IsAsync();

            DynamicMethod dynamicMethod = new DynamicMethod($"invoker_{Guid.NewGuid()}",
                isAsync ? typeof(Task) : typeof(void), new Type[] { typeof(AspectContext) }, method.Module, true);
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
                var returnLocal = il.DeclareLocal(method.ReturnType);
                il.Emit(OpCodes.Stloc, returnLocal);
                il.Emit(OpCodes.Ldloc, returnLocal);
                il.EmitConvertToObject(method.ReturnType);
                il.Emit(OpCodes.Call, ConstantInfo.SetReturnValue);
                if (isAsync)
                {
                    il.Emit(OpCodes.Ldloc, returnLocal);
                    if (method.IsReturnTask())
                    {
                        il.EmitConvertToType(method.ReturnType, typeof(Task));
                        il.Emit(OpCodes.Call, ConstantInfo.Await);
                    }
                    else if (method.IsValueTask())
                    {
                        il.Emit(OpCodes.Call, ConstantInfo.AwaitValueTask);
                    }
                    else if (method.IsReturnValueTask())
                    {
                        var returnType = method.ReturnType.GetTypeInfo().GetGenericArguments().Single();
                        il.Emit(OpCodes.Call, ConstantInfo.AwaitValueTaskReturnValue.MakeGenericMethod(returnType));
                    }
                    else
                    {
                        il.Emit(OpCodes.Call, ConstantInfo.Await);
                    }
                }
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
            return dynamicMethod;
        }

        public static async Task Await(Task task)
        {
            if (!task.IsCompleted)
            {
                await task;
            }
        }

        public static async Task AwaitValueTask(ValueTask task)
        {
            if (!task.IsCompleted)
            {
                await task;
            }
        }

        public static async Task AwaitValueTaskReturnValue<T>(ValueTask<T> task)
        {
            if (!task.IsCompleted)
            {
                await task;
            }
        }

        public AspectDelegateAsync GetInterceptorAsync(MethodInfo method, ProxyTypes proxyType)
        {
            AspectDelegateAsync baseCall;
            if (proxyType == ProxyTypes.Facade)
            {
                baseCall = CreateAsyncCaller(method);
            }
            else
            {
                var lazyCaller = new AsyncLazyCaller($"{method.Name}_Base");
                baseCall = lazyCaller.Call;
            }

            return FindInterceptors(method).Select(i =>
            {
                MAspectDelegateAsync a = i.InvokeAsync;
                return a;
            }).Aggregate(baseCall, (i, j) => c => j(c, i));
        }

        public AspectDelegateAsync GetGenericInterceptorAsync(MethodInfo method, ProxyTypes proxyType)
        {
            AspectDelegateAsync baseCall;
            var lazyCaller = new AsyncLazyGenericCaller(proxyType == ProxyTypes.Facade ? method.Name : $"{method.Name}_Base");
            baseCall = lazyCaller.Call;

            return FindInterceptors(method).Select(i =>
            {
                MAspectDelegateAsync a = i.InvokeAsync;
                return a;
            }).Aggregate(baseCall, (i, j) => c => j(c, i));
        }
    }
}