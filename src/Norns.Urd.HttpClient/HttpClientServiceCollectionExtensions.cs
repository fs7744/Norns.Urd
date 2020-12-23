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
                services.AddHttpClient()
                    .AddSingleton<IHttpClientHandler, HttpClientHandler>();
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHttpContentSerializer), typeof(SystemTextJsonContentSerializer)));
                services.TryAddTransient<OptionsCreator<JsonSerializerOptions>>();
                services.TryAddEnumerable(ServiceDescriptor.Singleton(typeof(IHttpContentSerializer), typeof(XmlContentSerializer)));
                services.TryAddTransient<OptionsCreator<XmlContentSerializerOptions>>();
            });
            return configuration;
        }
    }
}