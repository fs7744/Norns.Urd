using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public interface IHttpClientHandler
    {
        HttpClient CreateClient(string name);

        Task<T> DeserializeAsync<T>(HttpContent content);

        Task<HttpContent> SerializeAsync<T>(T data, string mediaType);
    }

    public class HttpClientHandler : IHttpClientHandler
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly Dictionary<string, IHttpContentSerializer> serializers;

        public HttpClientHandler(IHttpClientFactory clientFactory, IEnumerable<IHttpContentSerializer> serializers)
        {
            this.clientFactory = clientFactory;
            this.serializers = serializers.Reverse()
                .SelectMany(i => i.GetMediaTypes().Select(j => (mediaType: j, serializer: i)))
                .DistinctBy(i => i.mediaType)
                .ToDictionary(i => i.mediaType, i => i.serializer, StringComparer.OrdinalIgnoreCase);
        }

        public HttpClient CreateClient(string name)
        {
            return clientFactory.CreateClient(name);
        }

        public async Task<T> DeserializeAsync<T>(HttpContent content)
        {
            if (serializers.TryGetValue(content.Headers.ContentType.MediaType, out var serializer))
            {
                return await serializer.DeserializeAsync<T>(content);
            }
            else
            {
                throw new KeyNotFoundException($"Not found serializer: {content.Headers.ContentType.MediaType}.");
            }
        }

        public async Task<HttpContent> SerializeAsync<T>(T data, string mediaType)
        {
            if (serializers.TryGetValue(mediaType, out var serializer))
            {
                return await serializer.SerializeAsync<T>(data);
            }
            else
            {
                throw new KeyNotFoundException($"Not found serializer: {mediaType}.");
            }
        }
    }
}