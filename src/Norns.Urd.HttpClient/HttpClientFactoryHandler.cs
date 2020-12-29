using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{

    public interface IHttpClientFactoryHandler : IHttpClientHandler
    {
        HttpClient CreateClient(string name);

        Task<T> DeserializeAsync<T>(HttpContent content);

        Task<HttpContent> SerializeAsync<T>(T data, string mediaType);
    }

    public class HttpClientFactoryHandler : IHttpClientFactoryHandler
    {
        private readonly IHttpClientFactory clientFactory;
        private readonly Dictionary<string, IHttpContentSerializer> serializers;
        private readonly IHttpClientHandler[] handlers;

        public int Order => 0;

        public HttpClientFactoryHandler(IHttpClientFactory clientFactory, IEnumerable<IHttpContentSerializer> serializers, IEnumerable<IHttpClientHandler> handlers)
        {
            this.clientFactory = clientFactory;
            this.serializers = serializers.Reverse()
                .SelectMany(i => i.GetMediaTypes().Select(j => (mediaType: j, serializer: i)))
                .DistinctBy(i => i.mediaType)
                .ToDictionary(i => i.mediaType, i => i.serializer, StringComparer.OrdinalIgnoreCase);
            this.handlers = handlers.OrderBy(i => i.Order)
                .ToArray();
        }

        public HttpClient CreateClient(string name)
        {
            return clientFactory.CreateClient(name);
        }

        public async Task<T> DeserializeAsync<T>(HttpContent content)
        {
            if (content.Headers.ContentType == null)
            {
                return default;
            }
            else if (serializers.TryGetValue(content.Headers.ContentType.MediaType, out var serializer))
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

        public async Task SetRequestAsync(HttpRequestMessage message, AspectContext context)
        {
            foreach (var handler in handlers)
            {
                await handler.SetRequestAsync(message, context);
            }
        }

        public async Task SetResponseAsync(HttpResponseMessage resp, AspectContext context)
        {
            foreach (var handler in handlers)
            {
                await handler.SetResponseAsync(resp, context);
            }
        }
    }
}