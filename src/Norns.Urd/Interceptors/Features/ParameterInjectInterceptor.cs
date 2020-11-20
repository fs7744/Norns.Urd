using System;
using System.Linq;
using System.Reflection;
using Norns.Urd.Reflection;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace Norns.Urd.Interceptors.Features
{
    public class ParameterInjectInterceptor : IInterceptor
    {
        private static ConcurrentDictionary<MethodInfo, Action<AspectContext>> cache = new ConcurrentDictionary<MethodInfo, Action<AspectContext>>();
        public int Order => int.MinValue;

        public bool CanAspect(MethodInfo method)
        {
            return method.GetReflector().Parameters.Any(i => i.IsDefined<InjectAttribute>());
        }

        public void Invoke(AspectContext context, AspectDelegate next)
        {
            InjectParameters(context);
            next(context);
        }

        public async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            InjectParameters(context);
            await next(context);
        }

        private void InjectParameters(AspectContext context)
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