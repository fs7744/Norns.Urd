using Norns.Urd.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using System.Threading;

namespace Norns.Urd.Http
{
    internal class HttpMethodSettings : IHttpRequestMessageSettings
    {
        private readonly Func<HttpRequestMessage, AspectContext, CancellationToken, Task> setRequest;

        public int Order => int.MinValue;

        public HttpMethodSettings(IEnumerable<ParameterReflector> routes, IEnumerable<ParameterReflector> querys, string path, HttpMethod method, bool isDynamicPath)
        {
            var action = CreateUriReplacement(routes, querys, method);
            setRequest = isDynamicPath
                ? (request, context, token) =>
                {
                    context.ServiceProvider.GetRequiredService<IHttpRequestDynamicPathFactory>().GetDynamicPath(path, action)(request, context, token);
                    return Task.CompletedTask;
                }
                : action(path);
        }

        public Func<string, Func<HttpRequestMessage, AspectContext, CancellationToken, Task>> CreateUriReplacement(IEnumerable<ParameterReflector> routes, IEnumerable<ParameterReflector> querys, HttpMethod method)
        {
            var routeReplacements = routes.Select(i => ($"{{{i.GetCustomAttribute<RouteAttribute>().Alias ?? i.MemberInfo.Name}}}", $"{{{i.MemberInfo.Position}}}"))
                .ToArray();
            var queryReplacements = querys.Select(i => (i.GetCustomAttribute<QueryAttribute>(), i))
                .ToArray();
            var noRouteReplacements = routeReplacements.IsNullOrEmpty();
            var noQueryReplacements = queryReplacements.IsNullOrEmpty();
            if (noRouteReplacements && noQueryReplacements)
            {
                return s =>
                {
                    var uri = CreateUri(s);
                    return (request, context, token) => 
                    {
                        request.RequestUri = uri;
                        request.Method = method;
                        return Task.CompletedTask;
                    };
                };
            }
            else if (noQueryReplacements)
            {
                return s =>
                {
                    var p = routeReplacements.Aggregate(s, (x, y) =>
                    {
                        return x.Replace(y.Item1, y.Item2);
                    });
                    return (request, context, token) => 
                    {
                        request.RequestUri = CreateUri(string.Format(p, context.Parameters));
                        request.Method = method;
                        return Task.CompletedTask;
                    };
                };
            }
            else if (noRouteReplacements)
            {
                Lazy<Func<string, AspectContext, string>, AspectContext> lazyQueryStringBuilder = new Lazy<Func<string, AspectContext, string>, AspectContext>(c =>
                    c.ServiceProvider.GetRequiredService<IQueryStringBuilder>().Build(queryReplacements));
                return s =>
                {
                    return (request, context, token) =>
                    {
                        request.RequestUri = CreateUri(lazyQueryStringBuilder.GetValue(context)(s, context));
                        request.Method = method;
                        return Task.CompletedTask;
                    };
                };
            }
            else
            {
                Lazy<Func<string, AspectContext, string>, AspectContext> lazyQueryStringBuilder = new Lazy<Func<string, AspectContext, string>, AspectContext>(c =>
                    c.ServiceProvider.GetRequiredService<IQueryStringBuilder>().Build(queryReplacements));
                return s =>
                {
                    var p = routeReplacements.Aggregate(s, (x, y) =>
                    {
                        return x.Replace(y.Item1, y.Item2);
                    });
                    return (request, context, token) =>
                    {
                        request.RequestUri = CreateUri(lazyQueryStringBuilder.GetValue(context)(string.Format(p, context.Parameters), context));
                        request.Method = method;
                        return Task.CompletedTask;
                    };
                };
            }
        }

        public Uri CreateUri(string path)
        {
            return !string.IsNullOrEmpty(path)
               && Uri.TryCreate(path, UriKind.RelativeOrAbsolute, out var uri)
               ? uri
               : null;
        }

        public Task SetRequestAsync(HttpRequestMessage request, AspectContext context, CancellationToken cancellationToken)
        {
            return setRequest(request, context, cancellationToken);
        }
    }
}