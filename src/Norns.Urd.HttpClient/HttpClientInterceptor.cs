using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Norns.Urd.Reflection;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Linq;

namespace Norns.Urd.Http
{
    public class HttpClientInterceptor : AbstractInterceptor
    {
        private readonly Lazy<IHttpClientHandler, AspectContext> lazyClientFactory = new Lazy<IHttpClientHandler, AspectContext>(c => c.ServiceProvider.GetRequiredService<IHttpClientHandler>());
        private static readonly ConcurrentDictionary<MethodInfo, Func<AspectContext, Task>> asyncCache = new ConcurrentDictionary<MethodInfo, Func<AspectContext, Task>>();

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

        private Func<AspectContext, Task> CreateHttpClientCaller(MethodInfo method)
        {
            var mr = method.GetReflector();
            var tr = method.DeclaringType.GetReflector();
            var clientName = mr.GetCustomAttributesDistinctBy<ClientNameAttribute>(tr)
                .Select(i => i.Name)
                .FirstOrDefault() ?? Options.DefaultName;
            var clientSetters = mr.GetCustomAttributesDistinctBy<ClientSettingsAttribute>(tr).ToArray();
            var requestSetters = mr.GetCustomAttributesDistinctBy<HttpRequestMessageSettingsAttribute>(tr)
                .OrderBy(i => i.Order)
                .ToArray();
            var option = mr.GetCustomAttributesDistinctBy<HttpCompletionOptionAttribute>(tr)
                .Select(i => i.Option)
                .FirstOrDefault(HttpCompletionOption.ResponseHeadersRead);
            var contentType = mr.GetCustomAttributesDistinctBy<MediaTypeHeaderValueAttribute>(tr)
                .Select(i => i.ContentType)
                .FirstOrDefault(JsonContentTypeAttribute.Json);
            return async (context) => 
            {
                var handler = lazyClientFactory.GetValue(context);
                var client = handler.CreateClient(clientName);
                foreach (var setter in clientSetters)
                {
                    setter.SetClient(client, context);
                }
                var message = new HttpRequestMessage();
                //message.Content = await handler.SerializeAsync(, contentType);
                foreach (var setter in requestSetters)
                {
                    setter.SetRequest(message, context);
                }
                var resp = await client.SendAsync(message, option);
            };
        }
    }
}