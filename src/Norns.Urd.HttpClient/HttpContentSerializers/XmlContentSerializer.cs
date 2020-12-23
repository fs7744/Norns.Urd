using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Norns.Urd.Http
{
    public class XmlContentSerializer : IHttpContentSerializer
    {
        private readonly ConcurrentDictionary<Type, XmlSerializer> serializerCache = new ConcurrentDictionary<Type, XmlSerializer>();
        private readonly XmlContentSerializerOptions options;

        public XmlContentSerializer(OptionsCreator<XmlContentSerializerOptions> creator)
        {
            options = creator.Options ?? new XmlContentSerializerOptions();
        }

        private XmlSerializer GetXmlSerializer(Type type)
        {
            return serializerCache.GetOrAdd(type, t => new XmlSerializer(t, options.XmlAttributeOverrides));
        }

        public IEnumerable<string> GetMediaTypes()
        {
            yield return XmlContentTypeAttribute.Xml.MediaType;
            yield return "text/xml";
        }

        public async Task<T> DeserializeAsync<T>(HttpContent content)
        {
            var xmlSerializer = GetXmlSerializer(typeof(T));
            return (T)xmlSerializer.Deserialize(await content.ReadAsStreamAsync());
        }

        public Task<HttpContent> SerializeAsync<T>(T data)
        {
            var xmlSerializer = GetXmlSerializer(data.GetType());
            var stream = new MemoryStream();
            var writer = XmlWriter.Create(stream, options.WriterSettings);
            xmlSerializer.Serialize(writer, data, options.Namespaces);
            var content = new StreamContent(stream)
            {
                Headers =
                {
                    ContentLength = stream.Length,
                    ContentType = XmlContentTypeAttribute.Xml
                }
            };
            stream.Seek(0, SeekOrigin.Begin);
            return Task.FromResult((HttpContent)content);
        }
    }

    public class XmlContentSerializerOptions
    {
        public XmlSerializerNamespaces Namespaces { get; set; } = CreateDefaultNamespaces();

        public XmlWriterSettings WriterSettings { get; set; } = new XmlWriterSettings();

        public XmlReaderSettings ReaderSettings { get; set; } = new XmlReaderSettings();

        public XmlAttributeOverrides XmlAttributeOverrides { get; set; } = new XmlAttributeOverrides();

        private static XmlSerializerNamespaces CreateDefaultNamespaces()
        {
            var xmlnamespace = new XmlSerializerNamespaces();
            xmlnamespace.Add(string.Empty, string.Empty);
            return xmlnamespace;
        }
    }
}