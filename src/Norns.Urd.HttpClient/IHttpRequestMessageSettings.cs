using System.Net.Http;

namespace Norns.Urd.Http
{
    public interface IHttpRequestMessageSettings
    {
        int Order { get; }

        void SetRequest(HttpRequestMessage request, AspectContext context);
    }
}