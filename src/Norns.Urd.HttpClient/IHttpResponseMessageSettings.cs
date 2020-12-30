using System.Net.Http;

namespace Norns.Urd.Http
{
    public interface IHttpResponseMessageSettings
    {
        int Order { get; }

        void SetResponse(HttpResponseMessage resp, AspectContext context);
    }
}