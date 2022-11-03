using Norns.Urd.Reflection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Norns.Urd.Interceptors.Features
{
    public class ParameterInjectInterceptor : AbstractInterceptor
    {
        private static readonly ConcurrentDictionary<MethodInfo, Action<AspectContext>> cache = new ConcurrentDictionary<MethodInfo, Action<AspectContext>>();
        public override int Order => int.MinValue;

        public override bool CanAspect(MethodReflector method)
        {
            return method.Parameters.Any(i => i.IsDefined<InjectAttribute>());
        }

        public override void Invoke(AspectContext context, AspectDelegate next)
        {
            InjectParameters(context);
            next(context);
        }

        public override async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            InjectParameters(context);
            await next(context);
        }

        private static void InjectParameters(AspectContext context)
        {
            cache.GetOrAdd(context.Method, CreateInjectParametersAction)(context);
        }

        private static Action<AspectContext> CreateInjectParametersAction(MethodInfo m)
        {
            var ps = m.GetReflector().Parameters
                                .Where(i => i.IsDefined<InjectAttribute>())
                                .Select(i => i.MemberInfo)
                                .ToArray();
            return c =>
            {
                foreach (var p in ps)
                {
                    if (c.Parameters[p.Position] == null)
                    {
                        c.Parameters[p.Position] = c.ServiceProvider.GetService(p.ParameterType);
                    }
                }
            };
        }
    }
}