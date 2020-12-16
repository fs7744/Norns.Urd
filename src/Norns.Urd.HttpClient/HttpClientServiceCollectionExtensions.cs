using Norns.Urd;
using Norns.Urd.HttpClient;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class HttpClientServiceCollectionExtensions
    {
        public static IAspectConfiguration EnablePolly(this IAspectConfiguration configuration)
        {
            configuration.GlobalInterceptors.Add(new HttpClientInterceptor());
            configuration.ConfigServices.Add(services => services.AddHttpClient());
            return configuration;
        }
    }
}