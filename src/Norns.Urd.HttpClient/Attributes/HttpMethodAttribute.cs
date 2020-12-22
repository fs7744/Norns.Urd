﻿using System;
using System.Net.Http;

namespace Norns.Urd.Http
{

    public abstract class HttpMethodAttribute : HttpRequestMessageSettingsAttribute
    {
        private readonly string url;
        private readonly HttpMethod method;

        protected HttpMethodAttribute(string url, HttpMethod method)
        {
            this.url = url;
            this.method = method;
        }

        public override void SetRequest(HttpRequestMessage request, AspectContext context)
        {
            request.RequestUri = new Uri(url);
            request.Method = method;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GetAttribute : HttpMethodAttribute
    {
        public GetAttribute(string path) : base(path, HttpMethod.Get)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PostAttribute : HttpMethodAttribute
    {
        public PostAttribute(string path) : base(path, HttpMethod.Post)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PutAttribute : HttpMethodAttribute
    {
        public PutAttribute(string path) : base(path, HttpMethod.Put)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class DeleteAttribute : HttpMethodAttribute
    {
        public DeleteAttribute(string path) : base(path, HttpMethod.Delete)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class PatchAttribute : HttpMethodAttribute
    {
        public static readonly HttpMethod Patch = new HttpMethod("PATCH");

        public PatchAttribute(string path) : base(path, Patch)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class OptionsAttribute : HttpMethodAttribute
    {
        public OptionsAttribute(string path) : base(path, HttpMethod.Options)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class HeadAttribute : HttpMethodAttribute
    {
        public HeadAttribute(string path) : base(path, HttpMethod.Head)
        {
        }
    }
}