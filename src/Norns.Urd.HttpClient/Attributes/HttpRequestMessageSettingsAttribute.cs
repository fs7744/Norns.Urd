using System;
using System.Net.Http;

namespace Norns.Urd.Http
{
    public abstract class HttpRequestMessageSettingsAttribute : Attribute, IHttpRequestMessageSettings
    {
        public abstract void SetRequest(HttpRequestMessage request, AspectContext context);
    }
}