using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public interface IHttpRequestDynamicPathFactory
    {
        Func<HttpRequestMessage, AspectContext, CancellationToken, Task> GetDynamicPath(string key, Func<string, Func<HttpRequestMessage, AspectContext, CancellationToken, Task>> action);
    }
}