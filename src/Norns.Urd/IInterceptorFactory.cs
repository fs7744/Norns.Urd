using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Norns.Urd
{
    public interface IInterceptorFactory
    {
        void CreateInterceptor(MethodInfo method);
    }

    public class InterceptorFactory : IInterceptorFactory
    {
        private readonly ConcurrentDictionary<MethodInfo, AspectDelegate> syncInterceptors = new ConcurrentDictionary<MethodInfo, AspectDelegate>();
        private readonly IAspectConfiguration configuration;

        public InterceptorFactory(IAspectConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void CreateInterceptor(MethodInfo method)
        {
            AspectDelegate action = c => method.Invoke(c.Service, c.Parameters);
            var caller = configuration.Interceptors.Select(i => 
            {
                MAspectDelegate a =  i.Invoke;
                return a;
            }).Aggregate(action, (i, j) => c => j(c, i));

            syncInterceptors.TryAdd(method, caller);
        }

        public void CallInterceptor(AspectContext context)
        {
            syncInterceptors[context.ServiceMethod](context);
        }
    }
}
