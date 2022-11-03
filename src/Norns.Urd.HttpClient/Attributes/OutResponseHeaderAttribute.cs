using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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

        public override Task SetResponseAsync(HttpResponseMessage resp, AspectContext context, CancellationToken cancellationToken)
        {
            if (resp.Headers.TryGetValues(headerName, out var vs)
                || resp.Content.Headers.TryGetValues(headerName, out vs))
            {
                context.Parameters[Parameter.Position] = vs.FirstOrDefault();
            }
            return Task.CompletedTask;
        }
    }
}