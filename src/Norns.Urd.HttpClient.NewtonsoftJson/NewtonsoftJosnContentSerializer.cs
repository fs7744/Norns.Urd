using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Norns.Urd.Http;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.HttpClient
{
    public class NewtonsoftJosnContentSerializer : IHttpContentSerializer
    {
        private readonly JsonSerializer jsonSerializer;

        public NewtonsoftJosnContentSerializer(IOptions<JsonSerializerSettings> options)
        {
            jsonSerializer = JsonSerializer.CreateDefault(options.Value);
        }

        public async Task<T> DeserializeAsync<T>(HttpContent content, CancellationToken token)
        {
#if NET5_0
            using (var stream = await content.ReadAsStreamAsync(token))
#else
            using (var stream = await content.ReadAsStreamAsync())
#endif
            {
                var reader = new StreamReader(stream);
                var jsonReader = new JsonTextReader(reader);
                return jsonSerializer.Deserialize<T>(jsonReader);
            }
        }

        public IEnumerable<string> GetMediaTypes()
        {
            yield return JsonContentTypeAttribute.Json.MediaType;
            yield return "text/json";
        }

        public Task<HttpContent> SerializeAsync<T>(T data, CancellationToken token)
        {
            var stream = new MemoryStream();
            
            var writer = new StreamWriter(stream);
            jsonSerializer.Serialize(writer, data); 
            writer.Flush();
            var content = new StreamContent(stream)
            {
                Headers =
                {
                    ContentType = JsonContentTypeAttribute.Json
                }
            };
            content.Headers.ContentLength = stream.Length;
            stream.Seek(0, SeekOrigin.Begin);
            return Task.FromResult<HttpContent>(content);
        }
    }
}
