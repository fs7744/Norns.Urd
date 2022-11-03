using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public abstract class HttpResponseMessageSettingsAttribute : Attribute, IHttpResponseMessageSettings
    {
        public virtual int Order => 0;

        public abstract Task SetResponseAsync(HttpResponseMessage resp, AspectContext context, CancellationToken cancellationToken);
    }
}