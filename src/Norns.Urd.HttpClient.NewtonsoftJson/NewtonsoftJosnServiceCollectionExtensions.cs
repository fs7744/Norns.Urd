using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Norns.Urd.Http;
using Norns.Urd.HttpClient;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class NewtonsoftJosnServiceCollectionExtensions
    {
        public static IServiceCollection AddHttpClientNewtonsoftJosn(this IServiceCollection services)
        {
            services.TryAddEnumerable(ServiceDescriptor.Transient(typeof(IHttpContentSerializer), typeof(NewtonsoftJosnContentSerializer)));
            services.PostConfigure<JsonSerializerSettings>(i => { });
            return services;
        }
    }
}