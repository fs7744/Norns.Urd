using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Http.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class AuthenticationAttribute : ParameterRequestMessageSettingsAttribute
    {
        private readonly string scheme;

        public AuthenticationAttribute(string scheme = "Bearer")
        {
            this.scheme = scheme;
        }

        public override Task SetRequestAsync(HttpRequestMessage request, AspectContext context, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(scheme, context.Parameters[Parameter.Position]?.ToString());
            return Task.CompletedTask;
        }
    }
}