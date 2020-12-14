using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Reflection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Caching
{
    public class CacheInterceptor : AbstractInterceptor
    {
        private static readonly ConcurrentDictionary<MethodInfo, Action<AspectContext, AspectDelegate>> syncCache = new ConcurrentDictionary<MethodInfo, Action<AspectContext, AspectDelegate>>();
        private static readonly ConcurrentDictionary<MethodInfo, Func<AspectContext, AsyncAspectDelegate, Task>> asyncCache = new ConcurrentDictionary<MethodInfo, Func<AspectContext, AsyncAspectDelegate, Task>>();

        public override int Order { get; set; } = -95000;

        public override bool CanAspect(MethodReflector method)
        {
            return method.IsDefined<CacheAttribute>();
        }

        public override void Invoke(AspectContext context, AspectDelegate next)
        {
            syncCache.GetOrAdd(context.Method, CreateInvoke)(context, next);
        }

        private Action<AspectContext, AspectDelegate> CreateInvoke(MethodInfo method)
        {
            var returnType = method.ReturnType;
            if (returnType == typeof(void))
            {
                return (c, next) => next(c);
            }
            var optionCreators = method.GetReflector()
               .GetCustomAttributes<CacheAttribute>()
               .OrderBy(i => i.Order)
               .Select(i => i.GetCacheOptionCreator())
               .ToArray();
            var pt = typeof(ICacheProvider<>).MakeGenericType(returnType);
            return (context, next) =>
            {
                var p = context.ServiceProvider.GetRequiredService(pt) as ICacheProvider;
                context.ReturnValue = p.GetOrCreateValue(optionCreators, context, next);
            };
        }

        public override async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            await asyncCache.GetOrAdd(context.Method, CreateInvokeAsync)(context, next);
        }

        private static Func<AspectContext, AsyncAspectDelegate, Task> CreateInvokeAsync(MethodInfo method)
        {
            var returnType = method.ReturnType;
            if (returnType == typeof(Task) || returnType == typeof(ValueTask))
            {
                return (c, next) => next(c);
            }
            var mr = method.GetReflector();
            var optionCreators = mr
                .GetCustomAttributes<CacheAttribute>()
                .OrderBy(i => i.Order)
                .Select(i => i.GetCacheOptionCreator())
                .ToArray();
            var cti = mr.CancellationTokenIndex;
            var pt = typeof(ICacheProvider<>).MakeGenericType(returnType.GetGenericArguments()[0]);
            var isReturnValueTask = method.IsReturnValueTask();
            return async (context, next) =>
            {
                var token = cti < 0 ? CancellationToken.None : (CancellationToken)context.Parameters[cti];
                var p = context.ServiceProvider.GetRequiredService(pt) as ICacheProvider;
                var r = await p.GetOrCreateValueAsync(optionCreators, context, next, token, isReturnValueTask);
                context.ReturnValue = r;
            };
        }
    }
}