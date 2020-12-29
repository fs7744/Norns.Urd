using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public interface IHttpClientHandler
    {
        int Order { get; }

        Task SetRequestAsync(HttpRequestMessage message, AspectContext context, CancellationToken token);

        Task SetResponseAsync(HttpResponseMessage resp, AspectContext context, CancellationToken token);
    }

    public class EnsureSuccessStatusCodeHandler : IHttpClientHandler
    {
        public int Order => 0;

        public Task SetRequestAsync(HttpRequestMessage message, AspectContext context, CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public Task SetResponseAsync(HttpResponseMessage resp, AspectContext context, CancellationToken token)
        {
            resp.EnsureSuccessStatusCode();
            return Task.CompletedTask;
        }
    }
}