using System;
using System.Net.Http;

namespace Norns.Urd.Http
{
    public abstract class HttpResponseMessageSettingsAttribute : Attribute, IHttpResponseMessageSettings
    {
        public virtual int Order => 0;

        public abstract void SetResponse(HttpResponseMessage resp, AspectContext context);
    }
}