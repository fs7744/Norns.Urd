using Norns.Urd.Proxy;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Norns.Urd
{
    public interface IInterceptorFactory
    {
        void CreateInterceptor(MethodInfo method, AspectDelegate action, ProxyTypes proxyType = ProxyTypes.Facade);
    }

    public class InterceptorFactory : IInterceptorFactory
    {
        private readonly Dictionary<MethodInfo, AspectDelegate> syncFacadeInterceptors = new Dictionary<MethodInfo, AspectDelegate>();
        private readonly Dictionary<MethodInfo, AspectDelegate> syncInheritInterceptors = new Dictionary<MethodInfo, AspectDelegate>();
        private readonly IAspectConfiguration configuration;

        public InterceptorFactory(IAspectConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void CreateInterceptor(MethodInfo method, AspectDelegate action, ProxyTypes proxyType = ProxyTypes.Facade)
        {
            var caller = configuration.Interceptors.Select(i =>
            {
                MAspectDelegate a = i.Invoke;
                return a;
            }).Aggregate(action, (i, j) => c => j(c, i));
            var syncInterceptors = proxyType == ProxyTypes.Inherit ? syncInheritInterceptors : syncFacadeInterceptors;
            syncInterceptors.TryAdd(method, caller);
        }

        //private AspectDelegate CreateInheritInterceptor(MethodInfo method)
        //{
        //    AspectDelegate action;
        //    var dynamicMethod = new DynamicMethod($"C{Guid.NewGuid():N}", typeof(object), new[] { typeof(object), typeof(object[]) });
        //    var il = dynamicMethod.GetILGenerator();
        //    var m = typeof(object[]).GetMethod("get_Item");
        //    il.EmitLoadArg(0);
        //    for (int i = 0; i < method.GetParameters().Length; i++)
        //    {
        //        il.EmitLoadArg(1);
        //        il.EmitInt(i);
        //        il.EmitLoadElement(typeof(object));
        //    }

        //    il.Emit(OpCodes.Call, method);
        //    if (method.ReturnType == typeof(void))
        //    {
        //        il.Emit(OpCodes.Pop);
        //    }
        //    il.Emit(OpCodes.Ret);
        //    var func = dynamicMethod.CreateDelegate(typeof(Func<object, object[], object>)) as Func<object, object[], object>;
        //    action = c => c.ReturnValue = func(c.Service, c.Parameters);
        //    return configuration.Interceptors.Select(i =>
        //    {
        //        MAspectDelegate a = i.Invoke;
        //        return a;
        //    }).Aggregate(action, (i, j) => c => j(c, i));
        //}

        //private AspectDelegate CreateFacadeInterceptor(MethodInfo method)
        //{
        //    AspectDelegate action = c => c.ReturnValue = method.Invoke(c.Service, c.Parameters);
        //    return configuration.Interceptors.Select(i =>
        //    {
        //        MAspectDelegate a = i.Invoke;
        //        return a;
        //    }).Aggregate(action, (i, j) => c => j(c, i));
        //}

        public void CallInterceptor(AspectContext context)
        {
            var interceptor = context.ProxyType == ProxyTypes.Inherit ? syncInheritInterceptors : syncFacadeInterceptors;
            interceptor[context.ServiceMethod](context);
        }
    }
}