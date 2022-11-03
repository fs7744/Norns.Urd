using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public interface IHttpResponseMessageSettings
    {
        int Order { get; }

        Task SetResponseAsync(HttpResponseMessage resp, AspectContext context, CancellationToken cancellationToken);
    }
}