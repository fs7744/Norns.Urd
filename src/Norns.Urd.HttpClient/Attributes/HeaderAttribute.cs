using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

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

        public override Task SetRequestAsync(HttpRequestMessage request, AspectContext context, CancellationToken cancellationToken)
        {
            request.Headers.Add(name, value);
            return Task.CompletedTask;
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

        public override Task SetRequestAsync(HttpRequestMessage request, AspectContext context, CancellationToken cancellationToken)
        {
            request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(value));
            return Task.CompletedTask;
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class AcceptJsonAttribute : HttpRequestMessageSettingsAttribute
    {
        private static readonly MediaTypeWithQualityHeaderValue Json = MediaTypeWithQualityHeaderValue.Parse(JsonContentTypeAttribute.Json.MediaType);

        public override Task SetRequestAsync(HttpRequestMessage request, AspectContext context, CancellationToken cancellationToken)
        {
            request.Headers.Accept.Add(Json);
            return Task.CompletedTask;
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class AcceptXmlAttribute : HttpRequestMessageSettingsAttribute
    {
        private static readonly MediaTypeWithQualityHeaderValue Xml = MediaTypeWithQualityHeaderValue.Parse(XmlContentTypeAttribute.Xml.MediaType);

        public override Task SetRequestAsync(HttpRequestMessage request, AspectContext context, CancellationToken cancellationToken)
        {
            request.Headers.Accept.Add(Xml);
            return Task.CompletedTask;
        }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method, AllowMultiple = true)]
    public class AcceptOctetStreamAttribute : HttpRequestMessageSettingsAttribute
    {
        private static readonly MediaTypeWithQualityHeaderValue OctetStream = MediaTypeWithQualityHeaderValue.Parse(OctetStreamContentTypeAttribute.OctetStream.MediaType);

        public override Task SetRequestAsync(HttpRequestMessage request, AspectContext context, CancellationToken cancellationToken)
        {
            request.Headers.Accept.Add(OctetStream);
            return Task.CompletedTask;
        }
    }
}