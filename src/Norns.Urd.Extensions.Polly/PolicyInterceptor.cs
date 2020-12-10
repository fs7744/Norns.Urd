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
        private static readonly ConcurrentDictionary<MethodInfo, ISyncPolicy> syncCache = new ConcurrentDictionary<MethodInfo, ISyncPolicy>();

        public override int Order => -90000;

        public override bool CanAspect(MethodInfo method)
        {
            return method.GetReflector().IsDefined<AbstractPolicyAttribute>();
        }

        public override void Invoke(AspectContext context, AspectDelegate next)
        {
            syncCache.GetOrAdd(context.Method, CreateSyncPolicy)
                .Execute(() => next(context));
        }

        private ISyncPolicy CreateSyncPolicy(MethodInfo method)
        {
            var source = new CancellationTokenSource();
            var mr = method.GetReflector();
            return mr.GetCustomAttributes<AbstractPolicyAttribute>()
                .Select(i => i.Build(mr))
                .Where(i => i != null)
                .Aggregate(Policy.NoOp() as ISyncPolicy, (x, y) => x.Wrap(y));
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
                .Where(i => i != null)
                .Aggregate(Policy.NoOpAsync() as IAsyncPolicy, (x, y) => x.WrapAsync(y));
            var cancellationTokenIndex = mr.CancellationTokenIndex;
            Func<AspectContext, AsyncAspectDelegate, Task> executeAsync;
            if (cancellationTokenIndex > -1)
            {
                executeAsync = (context, next) =>
                {
                    var token = (CancellationToken)context.Parameters[cancellationTokenIndex];
                    return p.ExecuteAsync(ct =>
                    {
                        context.Parameters[cancellationTokenIndex] = ct;
                        return next(context);
                    }, token);
                };
            }
            else
            {
                executeAsync = (context, next) => p.ExecuteAsync(() => next(context));
            }
            return executeAsync;
        }
    }
}