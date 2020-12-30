using System;
using System.Net.Http;
using System.Net.Http.Headers;

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

        public override void SetRequest(HttpRequestMessage request, AspectContext context)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue(scheme, context.Parameters[Parameter.Position]?.ToString());
        }
    }
}