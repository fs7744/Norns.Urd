using Norns.Urd;
using Norns.Urd.Extensions.Polly;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class PollyServiceCollectionExtensions
    {
        public static IAspectConfiguration EnablePolly(this IAspectConfiguration configuration)
        {
            configuration.NonPredicates.AddNamespace("Polly")
                .AddNamespace("Polly.*");
            configuration.GlobalInterceptors.Add(new PolicyInterceptor());
            return configuration;
        }
    }
}