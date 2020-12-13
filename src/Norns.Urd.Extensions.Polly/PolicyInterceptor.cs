using Norns.Urd.Reflection;
using Polly;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Extensions.Polly
{
    public class PolicyInterceptor : AbstractInterceptor
    {
        private static readonly ConcurrentDictionary<MethodInfo, Func<AspectContext, AsyncAspectDelegate, Task>> asyncCache = new ConcurrentDictionary<MethodInfo, Func<AspectContext, AsyncAspectDelegate, Task>>();
        private static readonly ConcurrentDictionary<MethodInfo, Action<AspectContext, AspectDelegate>> syncCache = new ConcurrentDictionary<MethodInfo, Action<AspectContext, AspectDelegate>>();

        public override int Order => -90000;

        public override bool CanAspect(MethodInfo method)
        {
            var mr = method.GetReflector();
            return mr.IsDefined<AbstractPolicyAttribute>();
        }

        public override void Invoke(AspectContext context, AspectDelegate next)
        {
            syncCache.GetOrAdd(context.Method, CreateSyncPolicy)(context, next);
        }

        private Action<AspectContext, AspectDelegate> CreateSyncPolicy(MethodInfo method)
        {
            var mr = method.GetReflector();
            var p = mr.GetCustomAttributes<AbstractPolicyAttribute>()
                .Select(i => i.Build(mr))
                .Aggregate(Policy.NoOp() as ISyncPolicy, (x, y) => x.Wrap(y));
            return (context, next) => p.Execute(() => next(context));
        }

        public override async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            await asyncCache.GetOrAdd(context.Method, CreateAsyncPolicy)(context, next);
        }

        private Func<AspectContext, AsyncAspectDelegate, Task> CreateAsyncPolicy(MethodInfo method)
        {
            var mr = method.GetReflector();
            var p = mr.GetCustomAttributes<AbstractPolicyAttribute>()
                .Select(i => i.BuildAsync(mr))
                .Aggregate(Policy.NoOpAsync() as IAsyncPolicy, (x, y) => x.WrapAsync(y));
            var cancellationTokenIndex = mr.CancellationTokenIndex;
            Func<AspectContext, AsyncAspectDelegate, Task> executeAsync;
            if (cancellationTokenIndex > -1)
            {
                executeAsync = async (context, next) =>
                {
                    var token = (CancellationToken)context.Parameters[cancellationTokenIndex];
                    await p.ExecuteAsync(async (ct) =>
                    {
                        context.Parameters[cancellationTokenIndex] = ct;
                        await next(context);
                    }, token);
                };
            }
            else
            {
                executeAsync = async (context, next) =>
                {
                    await p.ExecuteAsync(async () => await next(context));
                };
            }
            return executeAsync;
        }
    }
}