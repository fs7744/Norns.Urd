using System.Net.Http;

namespace Norns.Urd.Http
{
    public interface IHttpRequestMessageSettings
    {
        void SetRequest(HttpRequestMessage request, AspectContext context);
    }
}