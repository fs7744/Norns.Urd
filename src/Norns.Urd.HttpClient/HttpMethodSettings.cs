using Norns.Urd.Reflection;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Norns.Urd.Http
{
    internal class HttpMethodSettings : IHttpRequestMessageSettings
    {
        private readonly Action<HttpRequestMessage, AspectContext> setRequest;
        public HttpMethodSettings(IEnumerable<ParameterReflector> routes, IEnumerable<ParameterReflector> querys, string path, HttpMethod method, bool isDynamicPath)
        {
            var action = CreateUriReplacement(routes, querys, method);
            setRequest = isDynamicPath
                ? (request, context) => context.ServiceProvider.GetRequiredService<IHttpRequestDynamicPathFactory>().GetDynamicPath(path, action)(request, context)
                : action(path);
        }

        public Func<string, Action<HttpRequestMessage, AspectContext>> CreateUriReplacement(IEnumerable<ParameterReflector> routes, IEnumerable<ParameterReflector> querys, HttpMethod method)
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
                    return (request, context) => 
                    {
                        request.RequestUri = uri;
                        request.Method = method;
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
                    return (request, context) => 
                    {
                        request.RequestUri = CreateUri(string.Format(p, context.Parameters));
                        request.Method = method;
                    };
                };
            }
            else if (noRouteReplacements)
            {
                Lazy<Func<string, AspectContext, string>, AspectContext> lazyQueryStringBuilder = new Lazy<Func<string, AspectContext, string>, AspectContext>(c =>
                    c.ServiceProvider.GetRequiredService<IQueryStringBuilder>().Build(queryReplacements));
                return s =>
                {
                    return (request, context) =>
                    {
                        request.RequestUri = CreateUri(lazyQueryStringBuilder.GetValue(context)(s, context));
                        request.Method = method;
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
                    return (request, context) =>
                    {
                        request.RequestUri = CreateUri(lazyQueryStringBuilder.GetValue(context)(string.Format(p, context.Parameters), context));
                        request.Method = method;
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

        public void SetRequest(HttpRequestMessage request, AspectContext context)
        {
            setRequest(request, context);
        }
    }
}