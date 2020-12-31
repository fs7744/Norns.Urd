using Norns.Urd.DynamicProxy;
using Norns.Urd.Reflection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Norns.Urd.Interceptors
{
    public interface IInterceptorCreator
    {
        AspectTypePredicate IsNonAspectType { get; }
        AspectMethodPredicate IsNonAspectMethod { get; }

        AspectDelegate GetInterceptor(MethodInfo method);

        AsyncAspectDelegate GetInterceptorAsync(MethodInfo method);

        AspectDelegate GetOpenGenericInterceptor(string methodName);

        AsyncAspectDelegate GetOpenGenericInterceptorAsync(string methodName);
    }

    public class InterceptorCreator : IInterceptorCreator
    {
        private readonly ConcurrentDictionary<MethodInfo, AspectDelegate> genericCallers = new ConcurrentDictionary<MethodInfo, AspectDelegate>();
        private readonly ConcurrentDictionary<MethodInfo, AsyncAspectDelegate> asyncGenericCallers = new ConcurrentDictionary<MethodInfo, AsyncAspectDelegate>();
        private readonly IAspectConfiguration configuration;
        public AspectTypePredicate IsNonAspectType { get; }

        public AspectMethodPredicate IsNonAspectMethod { get; }

        public InterceptorCreator(IAspectConfiguration configuration)
        {
            IsNonAspectType = configuration.NonPredicates.BuildNonAspectTypePredicate();
            IsNonAspectMethod = configuration.NonPredicates.BuildNonAspectMethodPredicate();
            this.configuration = configuration;
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
            il.Emit(OpCodes.Call, Constants.GetService);
            var parameters = method.GetParameters().Select(i => i.ParameterType).ToArray();
            var argsLocal = il.DeclareLocal(typeof(object[]));
            var frefs = new LocalBuilder[parameters.Length];
            if (parameters.Length > 0)
            {
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Call, Constants.GetParameters);
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
                        il.EmitConvertObjectTo(defType);
                        il.Emit(OpCodes.Stloc, fref);
                        il.Emit(OpCodes.Ldloca, fref);
                    }
                    else
                    {
                        il.EmitConvertObjectTo(parameters[i]);
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
                il.Emit(OpCodes.Call, Constants.SetReturnValue);
                if (isAsync)
                {
                    il.Emit(OpCodes.Ldloc, returnLocal);
                    if (method.IsReturnTask())
                    {
                        il.EmitConvertTo(method.ReturnType, typeof(Task));
                        il.Emit(OpCodes.Call, Constants.Await);
                    }
                    else if (method.IsValueTask())
                    {
                        il.Emit(OpCodes.Call, Constants.AwaitValueTask);
                    }
                    else if (method.IsReturnValueTask())
                    {
                        var returnType = method.ReturnType.GetTypeInfo().GetGenericArguments().Single();
                        il.Emit(OpCodes.Call, Constants.AwaitValueTaskReturnValue.MakeGenericMethod(returnType));
                    }
                    else
                    {
                        il.Emit(OpCodes.Call, Constants.Await);
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

        public IEnumerable<IInterceptor> FindInterceptors(MethodInfo method)
        {
            return configuration.GlobalInterceptors
                .Union(method.GetCustomAttributes<AbstractInterceptorAttribute>())
                .Union(method.DeclaringType.GetCustomAttributes<AbstractInterceptorAttribute>())
                .Where(i => i.CanAspect(method.GetReflector()))
                .OrderBy(i => i.Order);
        }

        public AspectDelegate GetInterceptor(MethodInfo method)
        {
            AspectDelegate baseCall = method.IsGenericMethodDefinition
                ? c =>
                {
                    genericCallers.GetOrAdd(c.Method, serviceMethod =>
                    {
                        var m = method.MakeGenericMethod(serviceMethod.GetGenericArguments());
                        return CreateSyncCaller(m);
                    })(c);
                }
            : CreateSyncCaller(method);
            return CreateAspectDelegate(method, baseCall);
        }

        public AsyncAspectDelegate GetInterceptorAsync(MethodInfo method)
        {
            AsyncAspectDelegate baseCall = method.IsGenericMethodDefinition
                ? async c =>
                {
                    await asyncGenericCallers.GetOrAdd(c.Method, serviceMethod =>
                    {
                        var m = method.MakeGenericMethod(serviceMethod.GetGenericArguments());
                        return CreateAsyncCaller(m);
                    })(c);
                }
            : CreateAsyncCaller(method);
            return CreateAsyncAspectDelegate(method, baseCall);
        }

        public AspectDelegate GetOpenGenericInterceptor(string methodName)
        {
            Dictionary<Type, MethodInfo> openGenericCallers = new Dictionary<Type, MethodInfo>();
            return c =>
            {
                var t = c.Service.GetType();
                if (!openGenericCallers.TryGetValue(t, out var m))
                {
                    m = t.GetMethod(methodName);
                    openGenericCallers[t] = m;
                }
                genericCallers.GetOrAdd(m, s => CreateAspectDelegate(s, CreateSyncCaller(s)))(c);
            };
        }

        private AspectDelegate CreateAspectDelegate(MethodInfo serviceMethod, AspectDelegate baseCall)
        {
            return FindInterceptors(serviceMethod).Select(i =>
            {
                CallAspectDelegate a = i.Invoke;
                return a;
            }).Aggregate(baseCall, (i, j) => c => j(c, i));
        }

        public AsyncAspectDelegate GetOpenGenericInterceptorAsync(string methodName)
        {
            Dictionary<Type, MethodInfo> openGenericCallers = new Dictionary<Type, MethodInfo>();
            return async c =>
            {
                var t = c.Service.GetType();
                if (!openGenericCallers.TryGetValue(t, out var m))
                {
                    m = t.GetMethod(methodName);
                    openGenericCallers[t] = m;
                }
                await asyncGenericCallers.GetOrAdd(m, s => CreateAsyncAspectDelegate(s, CreateAsyncCaller(s)))(c);
            };
        }

        private AsyncAspectDelegate CreateAsyncAspectDelegate(MethodInfo serviceMethod, AsyncAspectDelegate baseCall)
        {
            var interceptor = FindInterceptors(serviceMethod).Select(i =>
            {
                CallAsyncAspectDelegate a = i.InvokeAsync;
                return a;
            }).Aggregate(baseCall, (i, j) => c => j(c, i));
            return CreateTaskExceptionConvertor(serviceMethod, interceptor);
        }

        private AsyncAspectDelegate CreateTaskExceptionConvertor(MethodInfo serviceMethod, AsyncAspectDelegate aspectDelegate)
        {
            if (serviceMethod.IsReturnTask())
            {
                var type = serviceMethod.ReturnType.GetGenericArguments()[0];
                var method = Constants.TaskFromException.MakeGenericMethod(type);
                var m = new DynamicMethod(Guid.NewGuid().ToString("N"), typeof(object), new Type[] { typeof(Exception) });
                var il = m.GetILGenerator();
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Call, method);
                il.Emit(OpCodes.Ret);
                var caller = (Func<Exception, object>)m.CreateDelegate(typeof(Func<Exception, object>));
                return async c =>
                {
                    try
                    {
                        await aspectDelegate(c);
                    }
                    catch (Exception ex)
                    {
                        c.ReturnValue = caller(ex);
                    }
                };
            }
            else if (serviceMethod.IsReturnValueTask())
            {
                var type = serviceMethod.ReturnType.GetGenericArguments()[0];
                var method = Constants.ValueTaskExceptionConvert.MakeGenericMethod(type);
                var m = new DynamicMethod(Guid.NewGuid().ToString("N"), typeof(object), new Type[] { typeof(Exception) });
                var il = m.GetILGenerator();
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Call, method);
                il.Emit(OpCodes.Ret);
                var caller = (Func<Exception, object>)m.CreateDelegate(typeof(Func<Exception, object>));
                return async c =>
                {
                    try
                    {
                        await aspectDelegate(c);
                    }
                    catch (Exception ex)
                    {
                        c.ReturnValue = caller(ex);
                    }
                };
            }
            else
            {
                return aspectDelegate;
            }
        }

        public static object TaskExceptionConvert<T>(Exception ex)
        {
            return Task.FromException<T>(ex);
        }

        public static object ValueTaskExceptionConvert<T>(Exception ex)
        {
            return new ValueTask<T>(Task.FromException<T>(ex));
        }

        public static AspectDelegate CreateSyncCaller(MethodInfo method)
        {
            return (AspectDelegate)CreateCaller(method).CreateDelegate(typeof(AspectDelegate));
        }

        public static AsyncAspectDelegate CreateAsyncCaller(MethodInfo method)
        {
            return (AsyncAspectDelegate)CreateCaller(method).CreateDelegate(typeof(AsyncAspectDelegate));
        }
    }
}