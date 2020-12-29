using System;
using System.Net.Http;

namespace Norns.Urd.Http
{
    public interface IHttpRequestDynamicPathFactory
    {
        Action<HttpRequestMessage, AspectContext> GetDynamicPath(string key, Func<string, Action<HttpRequestMessage, AspectContext>> action);
    }
}