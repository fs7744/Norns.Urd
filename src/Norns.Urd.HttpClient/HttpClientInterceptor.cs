using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Norns.Urd.Reflection;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Client = System.Net.Http.HttpClient;
using System.Linq;

namespace Norns.Urd.HttpClient
{
    public class HttpClientInterceptor : AbstractInterceptor
    {
        private readonly Lazy<IHttpClientFactory, AspectContext> lazyClientFactory = new Lazy<IHttpClientFactory, AspectContext>(c => c.ServiceProvider.GetRequiredService<IHttpClientFactory>());
        private static readonly ConcurrentDictionary<MethodInfo, Func<AspectContext, AsyncAspectDelegate, Task>> asyncCache = new ConcurrentDictionary<MethodInfo, Func<AspectContext, AsyncAspectDelegate, Task>>();

        public override bool CanAspect(MethodReflector method)
        {
            return method.IsDefined<HttpMethodAttribute>();
        }

        public override void Invoke(AspectContext context, AspectDelegate next)
        {
            //lazyClientFactory.GetValue(context).CreateClient().BaseAddress Options.DefaultName
        }

        public override Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            throw new NotImplementedException();
            //asyncCache.GetOrAdd(context.Method, CreateHttpClientCaller);
        }

        private Func<AspectContext, Client> CreateHttpClientCaller(MethodInfo method)
        {
            var mr = method.GetReflector();
            var tr = method.DeclaringType.GetReflector();
            var clientName = mr.GetCustomAttributes<ClientNameAttribute>().Union(tr.GetCustomAttributes<ClientNameAttribute>()).DistinctBy(i => i.GetType()).Select(i => i.Name).DefaultIfEmpty(Options.DefaultName).First();
            mr.GetCustomAttributes<ClientConfigAttribute>().Union(tr.GetCustomAttributes<ClientConfigAttribute>()).DistinctBy(i => i.GetType());
            return (context) => 
            {
                var client = lazyClientFactory.GetValue(context).CreateClient(clientName);

                return client;
            };
        }
    }
}