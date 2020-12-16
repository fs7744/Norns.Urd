using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Reflection;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Norns.Urd.HttpClient
{
    public class HttpClientInterceptor : AbstractInterceptor
    {
        private readonly Lazy<IHttpClientFactory, AspectContext> lazyClientFactory = new Lazy<IHttpClientFactory, AspectContext>(c => c.ServiceProvider.GetRequiredService<IHttpClientFactory>());

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
        }
    }
}