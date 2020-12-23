using System;
using System.Net.Http.Headers;
using System.Text;

namespace Norns.Urd.Http
{
    public abstract class MediaTypeHeaderValueAttribute : Attribute
    {
        protected MediaTypeHeaderValueAttribute(MediaTypeHeaderValue contentType)
        {
            ContentType = contentType;
        }

        public MediaTypeHeaderValue ContentType { get; }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class ContentTypeAttribute : MediaTypeHeaderValueAttribute
    {
        public ContentTypeAttribute(string contentType) : base(new MediaTypeHeaderValue(contentType))
        {
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class JsonContentTypeAttribute : MediaTypeHeaderValueAttribute
    {
        public static readonly MediaTypeHeaderValue Json = new MediaTypeHeaderValue("application/json") { CharSet = Encoding.UTF8.WebName };

        public JsonContentTypeAttribute() : base(Json)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class XmlContentTypeAttribute : MediaTypeHeaderValueAttribute
    {
        public static readonly MediaTypeHeaderValue Xml = new MediaTypeHeaderValue("application/xml");

        public XmlContentTypeAttribute() : base(Xml)
        {
        }
    }
}