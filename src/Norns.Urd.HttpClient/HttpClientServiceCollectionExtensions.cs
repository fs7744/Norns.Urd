using Norns.Urd;
using Norns.Urd.Http;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HttpClientServiceCollectionExtensions
    {
        public static IAspectConfiguration EnableHttpClient(this IAspectConfiguration configuration)
        {
            configuration.GlobalInterceptors.Add(new HttpClientInterceptor());
            configuration.ConfigServices.Add(services => services.AddHttpClient().AddSingleton<IHttpClientHandler, HttpClientHandler>());
            return configuration;
        }
    }
}