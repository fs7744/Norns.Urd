using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public class OctetStreamContentSerializer : IHttpContentSerializer
    {
        private readonly ILogger<OctetStreamContentSerializer> logger;

        public OctetStreamContentSerializer(ILogger<OctetStreamContentSerializer> logger)
        {
            this.logger = logger;
        }

        public async Task<T> DeserializeAsync<T>(HttpContent content, CancellationToken token)
        {
#if NET5_0
            return (T)(object)await content.ReadAsStreamAsync(token);
#else
             return (T)(object)await content.ReadAsStreamAsync();
#endif
        }

        public IEnumerable<string> GetMediaTypes()
        {
            yield return OctetStreamContentTypeAttribute.OctetStream.MediaType;
        }

        public Task<HttpContent> SerializeAsync<T>(T data, CancellationToken token)
        {
            if (data is Stream stream)
            {
                var content = new StreamContent(stream)
                {
                    Headers =
                {
                    ContentType = JsonContentTypeAttribute.Json
                }
                };
                try
                {
                    content.Headers.ContentLength = stream.Length;
                    stream.Seek(0, SeekOrigin.Begin);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Not supported.");
                }
                return Task.FromResult<HttpContent>(content);
            }
            else
            {
                throw new NotSupportedException(typeof(T).FullName);
            }
        }
    }
}