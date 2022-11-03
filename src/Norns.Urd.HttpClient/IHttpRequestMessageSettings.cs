using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public interface IHttpRequestMessageSettings
    {
        int Order { get; }

        Task SetRequestAsync(HttpRequestMessage request, AspectContext context, CancellationToken cancellationToken);
    }
}