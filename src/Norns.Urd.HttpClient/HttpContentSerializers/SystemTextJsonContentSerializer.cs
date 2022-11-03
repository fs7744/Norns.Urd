using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public class SystemTextJsonContentSerializer : IHttpContentSerializer
    {
        private readonly IOptions<JsonSerializerOptions> options;

        public SystemTextJsonContentSerializer(IOptions<JsonSerializerOptions> options)
        {
            this.options = options;
        }

        public async Task<T> DeserializeAsync<T>(HttpContent content, CancellationToken token)
        {
#if NET5_0
            using (var stream = await content.ReadAsStreamAsync(token))
#else
            using (var stream = await content.ReadAsStreamAsync())
#endif
            {
                return await JsonSerializer.DeserializeAsync<T>(stream, options.Value, token);
            }
        }

        public async Task<HttpContent> SerializeAsync<T>(T data, CancellationToken token)
        {
            //todo: poolbufferstream
            var stream = new MemoryStream();
            var content = new StreamContent(stream)
            {
                Headers =
                {
                    ContentType = JsonContentTypeAttribute.Json
                }
            };
            await JsonSerializer.SerializeAsync<T>(stream, data, options.Value, token);
            content.Headers.ContentLength = stream.Length;
            stream.Seek(0, SeekOrigin.Begin);
            return content;
        }

        public IEnumerable<string> GetMediaTypes()
        {
            yield return JsonContentTypeAttribute.Json.MediaType;
            yield return "text/json";
        }
    }
}