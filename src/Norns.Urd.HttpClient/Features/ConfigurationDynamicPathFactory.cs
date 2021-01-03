using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public class ConfigurationDynamicPathFactory : IHttpRequestDynamicPathFactory
    {
        private readonly IConfiguration configuration;
        private static readonly ConcurrentDictionary<string, Func<HttpRequestMessage, AspectContext, CancellationToken, Task>> cache = new ConcurrentDictionary<string, Func<HttpRequestMessage, AspectContext, CancellationToken, Task>>();


        public ConfigurationDynamicPathFactory(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Func<HttpRequestMessage, AspectContext, CancellationToken, Task> GetDynamicPath(string key, Func<string, Func<HttpRequestMessage, AspectContext, CancellationToken, Task>> action)
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