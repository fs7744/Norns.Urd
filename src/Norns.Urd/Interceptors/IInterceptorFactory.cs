using Norns.Urd.Proxy;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Norns.Urd
{
    public interface IInterceptorFactory
    {
        AspectDelegate GetInterceptor(MethodInfo method, ProxyTypes proxyType);

        AspectDelegateAsync GetInterceptorAsync(MethodInfo method, ProxyTypes proxyType);
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

        public AspectDelegateAsync GetInterceptorAsync(MethodInfo method, ProxyTypes proxyType)
        {
            var baseMethodName = $"{method.Name}_Base";
            AspectDelegateAsync baseCall;
            if (proxyType == ProxyTypes.Facade)
            {
                baseCall = async c => 
                {
                    c.ReturnValue = method.Invoke(c.Service, c.Parameters);

                    switch (c.ReturnValue)
                    {
                        case Task t:
                            await t;
                            break;
                        case ValueTask t:
                            await t;
                            break;
                        default:
                            break;
                    }
                };
            }
            else
            {
                baseCall = async c => 
                {
                    c.ReturnValue = c.Service.GetType().GetMethod(baseMethodName).Invoke(c.Service, c.Parameters);

                    switch (c.ReturnValue)
                    {
                        case Task t:
                            await t;
                            break;
                        case ValueTask t:
                            await t;
                            break;
                        default:
                            break;
                    }
                };
            }

            return configuration.Interceptors.Select(i =>
            {
                MAspectDelegateAsync a = i.InvokeAsync;
                return a;
            }).Aggregate(baseCall, (i, j) => c => j(c, i));
        }
    }
}