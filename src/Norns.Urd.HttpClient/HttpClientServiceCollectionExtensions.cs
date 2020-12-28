using Norns.Urd;
using Norns.Urd.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
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
                services.TryAddSingleton<IHttpClientHandler, HttpClientHandler>();
                services.TryAddSingleton<IHttpRequestDynamicPathFactory, ConfigurationDynamicPathFactory>();
                services.TryAddSingleton<IQueryStringBuilder, QueryStringBuilder>();
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHttpContentSerializer), typeof(SystemTextJsonContentSerializer)));
                services.TryAddTransient<OptionsCreator<JsonSerializerOptions>>(i => new OptionsCreator<JsonSerializerOptions>(() => 
                {
                    return new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    };
                }));
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHttpContentSerializer), typeof(XmlContentSerializer)));
                services.TryAddTransient<OptionsCreator<XmlContentSerializerOptions>>(i => 
                    new OptionsCreator<XmlContentSerializerOptions>(() => new XmlContentSerializerOptions()));
            });
            return configuration;
        }
    }
}