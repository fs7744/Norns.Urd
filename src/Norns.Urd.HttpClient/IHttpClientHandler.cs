using System.Net.Http;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public interface IHttpClientHandler
    {
        int Order { get; }

        Task SetRequestAsync(HttpRequestMessage message, AspectContext context);

        Task SetResponseAsync(HttpResponseMessage resp, AspectContext context);
    }

    public class EnsureSuccessStatusCodeHandler : IHttpClientHandler
    {
        public int Order => 0;

        public Task SetRequestAsync(HttpRequestMessage message, AspectContext context)
        {
            return Task.CompletedTask;
        }

        public Task SetResponseAsync(HttpResponseMessage resp, AspectContext context)
        {
            resp.EnsureSuccessStatusCode();
            return Task.CompletedTask;
        }
    }
}