using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;

namespace Norns.Urd.Http
{
    public interface IHttpContentSerializer
    {
        IEnumerable<string> GetMediaTypes();

        Task<T> DeserializeAsync<T>(HttpContent content, CancellationToken token);

        Task<HttpContent> SerializeAsync<T>(T data, CancellationToken token);
    }
}