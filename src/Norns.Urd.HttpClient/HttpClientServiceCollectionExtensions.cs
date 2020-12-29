using Microsoft.Extensions.DependencyInjection.Extensions;
using Norns.Urd;
using Norns.Urd.Http;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HttpClientServiceCollectionExtensions
    {
        public static IAspectConfiguration EnableHttpClient(this IAspectConfiguration configuration)
        {
            configuration.GlobalInterceptors.Add(new HttpClientInterceptor());
            configuration.ConfigServices.Add(services =>
            {
                services.AddHttpClient();
                services.TryAddSingleton<IHttpClientFactoryHandler, HttpClientFactoryHandler>();
                services.TryAddSingleton<IHttpRequestDynamicPathFactory, ConfigurationDynamicPathFactory>();
                services.TryAddSingleton<IQueryStringBuilder, QueryStringBuilder>();
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHttpContentSerializer), typeof(SystemTextJsonContentSerializer)));
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHttpClientHandler), typeof(EnsureSuccessStatusCodeHandler)));
                services.PostConfigure<JsonSerializerOptions>(i =>
                {
                    i.PropertyNameCaseInsensitive = true;
                    i.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                });
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHttpContentSerializer), typeof(XmlContentSerializer)));
                services.PostConfigure<XmlContentSerializerOptions>(i => { });
            });
            return configuration;
        }
    }
}