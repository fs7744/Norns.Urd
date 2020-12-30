using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Norns.Urd.Http
{
    public abstract class HttpResponseMessageSettingsAttribute : Attribute, IHttpResponseMessageSettings
    {
        public virtual int Order => 0;

        public abstract void SetResponse(HttpResponseMessage resp, AspectContext context);
    }

    public abstract class ParameterResponseMessageSettingsAttribute : HttpResponseMessageSettingsAttribute
    {
        public ParameterInfo Parameter { get; internal set; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class OutResponseHeaderAttribute : ParameterResponseMessageSettingsAttribute
    {
        private readonly string headerName;

        public OutResponseHeaderAttribute(string headerName)
        {
            this.headerName = headerName;
        }

        public override void SetResponse(HttpResponseMessage resp, AspectContext context)
        {
            if (resp.Headers.TryGetValues(headerName, out var vs)
                || resp.Content.Headers.TryGetValues(headerName, out vs))
            {
                context.Parameters[Parameter.Position] = vs.FirstOrDefault();
            }
        }
    }
}