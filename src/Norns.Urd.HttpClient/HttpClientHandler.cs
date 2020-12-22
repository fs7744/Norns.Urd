using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public interface IHttpClientHandler
    {
        HttpClient CreateClient(string name);

        Task<T> DeserializeAsync<T>(HttpContent content);

        Task<HttpContent> SerializeAsync<T>(T data);
    }

    public class HttpClientHandler : IHttpClientHandler
    {
        private readonly IHttpClientFactory clientFactory;

        public HttpClientHandler(IHttpClientFactory clientFactory)
        {
            this.clientFactory = clientFactory;
        }

        public HttpClient CreateClient(string name)
        {
            return clientFactory.CreateClient(name);
        }

        public Task<T> DeserializeAsync<T>(HttpContent content)
        {
            throw new NotImplementedException();
        }

        public Task<HttpContent> SerializeAsync<T>(T data)
        {
            throw new NotImplementedException();
        }
    }
}