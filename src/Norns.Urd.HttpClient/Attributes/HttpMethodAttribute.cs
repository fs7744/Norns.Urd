using Norns.Urd.Reflection;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Norns.Urd.Http
{

    public abstract class HttpMethodAttribute : Attribute
    {
        private readonly string path;
        private readonly HttpMethod method;
        public bool IsDynamicPath { get; set; }

        protected HttpMethodAttribute(string path, HttpMethod method)
        {
            this.path = path;
            this.method = method;
        }

        public IHttpRequestMessageSettings CreateSettings(IEnumerable<ParameterReflector> routes, IEnumerable<ParameterReflector> querys)
        {
            return new HttpMethodSettings(routes, querys, path, method, IsDynamicPath);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GetAttribute : HttpMethodAttribute
    {
        public GetAttribute(string path = null) : base(path, HttpMethod.Get)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostAttribute : HttpMethodAttribute
    {
        public PostAttribute(string path = null) : base(path, HttpMethod.Post)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PutAttribute : HttpMethodAttribute
    {
        public PutAttribute(string path = null) : base(path, HttpMethod.Put)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class DeleteAttribute : HttpMethodAttribute
    {
        public DeleteAttribute(string path = null) : base(path, HttpMethod.Delete)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PatchAttribute : HttpMethodAttribute
    {
        public static readonly HttpMethod Patch = new HttpMethod("PATCH");

        public PatchAttribute(string path = null) : base(path, Patch)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OptionsAttribute : HttpMethodAttribute
    {
        public OptionsAttribute(string path = null) : base(path, HttpMethod.Options)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HeadAttribute : HttpMethodAttribute
    {
        public HeadAttribute(string path = null) : base(path, HttpMethod.Head)
        {
        }
    }
}