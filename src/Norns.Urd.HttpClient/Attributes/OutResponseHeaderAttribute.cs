using System;
using System.Linq;
using System.Net.Http;

namespace Norns.Urd.Http
{
    [AttributeUsage(AttributeTargets.Parameter)]
    public class OutResponseHeaderAttribute : ParameterResponseMessageSettingsAttribute
    {
        private readonly string headerName;

        public OutResponseHeaderAttribute(string headerName)
        {
            this.headerName = headerName;
        }

        public override void SetResponse(HttpResponseMessage resp, AspectContext context)
        {
            if (resp.Headers.TryGetValues(headerName, out var vs)
                || resp.Content.Headers.TryGetValues(headerName, out vs))
            {
                context.Parameters[Parameter.Position] = vs.FirstOrDefault();
            }
        }
    }
}