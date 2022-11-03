using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public abstract class HttpRequestMessageSettingsAttribute : Attribute, IHttpRequestMessageSettings
    {
        public virtual int Order => 0;

        public abstract Task SetRequestAsync(HttpRequestMessage request, AspectContext context, CancellationToken cancellationToken);
    }
}