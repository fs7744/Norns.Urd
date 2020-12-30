using System;
using System.Net.Http;

namespace Norns.Urd.Http.Attributes
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class SetRequestHeaderAttribute : ParameterRequestMessageSettingsAttribute
    {
        private readonly string header;

        public SetRequestHeaderAttribute(string header)
        {
            this.header = header;
        }

        public override void SetRequest(HttpRequestMessage request, AspectContext context)
        {
            request.Headers.Add(header, context.Parameters[Parameter.Position]?.ToString());
        }
    }
}