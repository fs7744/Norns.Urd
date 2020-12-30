using System;
using System.Net.Http;

namespace Norns.Urd.Http
{
    public abstract class HttpRequestMessageSettingsAttribute : Attribute, IHttpRequestMessageSettings
    {
        public virtual int Order => 0;

        public abstract void SetRequest(HttpRequestMessage request, AspectContext context);
    }
}