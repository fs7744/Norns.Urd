using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Norns.Urd.Http
{
    public interface IHttpContentSerializer
    {
        IEnumerable<string> GetMediaTypes();

        Task<T> DeserializeAsync<T>(HttpContent content);

        Task<HttpContent> SerializeAsync<T>(T data);
    }
}