using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public class SystemTextJsonContentSerializer : IHttpContentSerializer
    {
        private readonly JsonSerializerOptions options;

        public SystemTextJsonContentSerializer(OptionsCreator<JsonSerializerOptions> creator)
        {
            options = creator.Options;
        }

        public async Task<T> DeserializeAsync<T>(HttpContent content)
        {
            return await JsonSerializer.DeserializeAsync<T>(await content.ReadAsStreamAsync(), options);
        }

        public async Task<HttpContent> SerializeAsync<T>(T data)
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
            await JsonSerializer.SerializeAsync<T>(stream, data, options);
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