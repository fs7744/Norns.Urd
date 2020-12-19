using System;
using System.Net.Http;

namespace Norns.Urd.HttpClient
{
    public abstract class HttpRequestMessageSettingsAttribute : Attribute
    {
        public abstract void SetRequest(HttpRequestMessage request, AspectContext context);
    }
}