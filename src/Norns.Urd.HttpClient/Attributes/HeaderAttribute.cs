using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Norns.Urd.Http
{
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class HeaderAttribute : HttpRequestMessageSettingsAttribute
    {
        private readonly string name;
        private readonly string value;

        public HeaderAttribute(string name, string value)
        {
            this.name = name;
            this.value = value;
        }

        public override void SetRequest(HttpRequestMessage request, AspectContext context)
        {
            request.Headers.Add(name, value);
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class AcceptAttribute : HttpRequestMessageSettingsAttribute
    {
        private readonly string value;

        public AcceptAttribute(string value)
        {
            this.value = value;
        }

        public override void SetRequest(HttpRequestMessage request, AspectContext context)
        {
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(value));
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class AcceptJsonAttribute : HttpRequestMessageSettingsAttribute
    {
        private static readonly MediaTypeWithQualityHeaderValue Json = MediaTypeWithQualityHeaderValue.Parse(JsonContentTypeAttribute.Json.MediaType);

        public override void SetRequest(HttpRequestMessage request, AspectContext context)
        {
            request.Headers.Accept.Add(Json);
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class AcceptXmlAttribute : HttpRequestMessageSettingsAttribute
    {
        private static readonly MediaTypeWithQualityHeaderValue Xml = MediaTypeWithQualityHeaderValue.Parse(XmlContentTypeAttribute.Xml.MediaType);

        public override void SetRequest(HttpRequestMessage request, AspectContext context)
        {
            request.Headers.Accept.Add(Xml);
        }
    }
}