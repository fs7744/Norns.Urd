using Norns.Urd.Proxy;
using System.Linq;
using System.Reflection;

namespace Norns.Urd
{
    public interface IInterceptorFactory
    {
        AspectDelegate GetInterceptor(MethodInfo method, ProxyTypes proxyType);
    }

    public class InterceptorFactory : IInterceptorFactory
    {
        private readonly IAspectConfiguration configuration;

        public InterceptorFactory(IAspectConfiguration configuration)
        {
            this.configuration = configuration;
        }

        // todo : 性能优化
        public AspectDelegate GetInterceptor(MethodInfo method, ProxyTypes proxyType)
        {
            var baseMethodName = $"{method.Name}_Base";
            AspectDelegate baseCall;
            if (proxyType == ProxyTypes.Facade)
            {
                baseCall = c => c.ReturnValue = method.Invoke(c.Service, c.Parameters);
            }
            else
            {
                baseCall = c => c.ReturnValue = c.Service.GetType().GetMethod(baseMethodName).Invoke(c.Service, c.Parameters);
            }

            return configuration.Interceptors.Select(i =>
            {
                MAspectDelegate a = i.Invoke;
                return a;
            }).Aggregate(baseCall, (i, j) => c => j(c, i));
        }
    }
}