using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

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

        public override Task SetRequestAsync(HttpRequestMessage request, AspectContext context, CancellationToken cancellationToken)
        {
            request.Headers.Add(header, context.Parameters[Parameter.Position]?.ToString());
            return Task.CompletedTask;
        }
    }
}