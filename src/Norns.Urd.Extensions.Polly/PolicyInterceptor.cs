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
            return mr.IsDefined<AbstractPolicyAttribute>() || mr.IsDefined<AbstractLazyPolicyAttribute>();
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
            var lazys = mr.GetCustomAttributes<AbstractLazyPolicyAttribute>()
                .Select(i => i.LazyBuild())
                .ToArray();
            var contextKeyGenerator = FindContextKeyGenerator(mr);
            var lazySyncPolicy = new Lazy<ISyncPolicy, AspectContext>(c =>
            {
                ISyncPolicy result = p;
                if (lazys.Length > 0)
                {
                    result = result.Wrap(lazys.Aggregate(result, (x, y) => x.Wrap(y.GetValue(c))));
                }
                lazys = null;
                return result;
            });
            return (context, next) =>
            {
                var o = lazySyncPolicy.GetValue(context)
                    .Execute(ct =>
                    {
                        next(context);
                        return context.ReturnValue;
                    }, new Context(contextKeyGenerator.GenerateKey(context)));
                context.ReturnValue = o;
            };
        }

        private IContextKeyGenerator FindContextKeyGenerator(MethodReflector mr)
        {
            var contextKeyGenerator = mr.GetCustomAttributes<AbstractContextKeyGeneratorAttribute>().FirstOrDefault();
            if (contextKeyGenerator == null)
            {
                contextKeyGenerator = new MethodNameKeyAttribute();
            }
            return contextKeyGenerator;
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
            var lazys = mr.GetCustomAttributes<AbstractLazyPolicyAttribute>()
                .Select(i => i.LazyBuildAsync())
                .ToArray();
            var contextKeyGenerator = FindContextKeyGenerator(mr);
            var lazyAsyncPolicy = new Lazy<IAsyncPolicy, AspectContext>(c =>
            {
                IAsyncPolicy result = p;
                if (lazys.Length > 0)
                {
                    result = result.WrapAsync(lazys.Aggregate(result, (x, y) => x.WrapAsync(y.GetValue(c))));
                }
                lazys = null;
                return result;
            });
            var cancellationTokenIndex = mr.CancellationTokenIndex;
            Func<AspectContext, AsyncAspectDelegate, Task> executeAsync;
            if (cancellationTokenIndex > -1)
            {
                executeAsync = async (context, next) =>
                {
                    var token = (CancellationToken)context.Parameters[cancellationTokenIndex];
                    var o = await lazyAsyncPolicy.GetValue(context).ExecuteAsync(async (c, ct) =>
                    {
                        context.Parameters[cancellationTokenIndex] = ct;
                        await next(context);
                        return context.ReturnValue;
                    }, new Context(contextKeyGenerator.GenerateKey(context)), token);
                    context.ReturnValue = o;
                };
            }
            else
            {
                executeAsync = async (context, next) =>
                {
                    var o = await lazyAsyncPolicy.GetValue(context).ExecuteAsync(async c =>
                    {
                        await next(context);
                        return context.ReturnValue;
                    }, new Context(contextKeyGenerator.GenerateKey(context)));
                    context.ReturnValue = o;
                };
            }
            return executeAsync;
        }
    }
}