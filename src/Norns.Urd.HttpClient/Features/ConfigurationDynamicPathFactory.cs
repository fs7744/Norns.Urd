using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Net.Http;

namespace Norns.Urd.Http
{
    public class ConfigurationDynamicPathFactory : IHttpRequestDynamicPathFactory
    {
        private readonly IConfiguration configuration;
        private static readonly ConcurrentDictionary<string, Action<HttpRequestMessage, AspectContext>> cache = new ConcurrentDictionary<string, Action<HttpRequestMessage, AspectContext>>();


        public ConfigurationDynamicPathFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Action<HttpRequestMessage, AspectContext> GetDynamicPath(string key, Func<string, Action<HttpRequestMessage, AspectContext>> action)
        {
            return cache.GetOrAdd(key, k =>
            {
                var section = configuration.GetSection(k);
                section.GetReloadToken()
                .RegisterChangeCallback(c => 
                {
                    cache.AddOrUpdate(k, action(configuration[k]), (x, y) => y);
                }, cache);
                return action(section.Value);
            });
        }
    }
}