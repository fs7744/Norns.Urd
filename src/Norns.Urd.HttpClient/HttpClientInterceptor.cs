using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Norns.Urd.DynamicProxy;
using Norns.Urd.Reflection;
using Norns.Urd.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Norns.Urd.Http
{
    public class HttpClientInterceptor : AbstractInterceptor
    {
        private static readonly MethodInfo SerializeAsync = typeof(IHttpClientHandler).GetMethod(nameof(IHttpClientHandler.SerializeAsync));
        private static readonly MethodInfo CallDeserialize = typeof(HttpClientInterceptor).GetMethod(nameof(HttpClientInterceptor.Deserialize));
        private static readonly MethodInfo CallDeserializeTaskAsync = typeof(HttpClientInterceptor).GetMethod(nameof(HttpClientInterceptor.DeserializeTaskAsync));
        private static readonly MethodInfo CallDeserializeValueTaskAsync = typeof(HttpClientInterceptor).GetMethod(nameof(HttpClientInterceptor.DeserializeValueTaskAsync));
        private readonly Lazy<IHttpClientHandler, AspectContext> lazyClientFactory = new Lazy<IHttpClientHandler, AspectContext>(c => c.ServiceProvider.GetRequiredService<IHttpClientHandler>());
        private static readonly ConcurrentDictionary<MethodInfo, Func<AspectContext, Task>> asyncCache = new ConcurrentDictionary<MethodInfo, Func<AspectContext, Task>>();

        public override bool CanAspect(MethodReflector method)
        {
            return method.IsDefined<HttpMethodAttribute>();
        }

        public override void Invoke(AspectContext context, AspectDelegate next)
        {
            asyncCache.GetOrAdd(context.Method, CreateHttpClientCaller)(context)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        public override async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            await asyncCache.GetOrAdd(context.Method, CreateHttpClientCaller)(context);
        }

        private Func<AspectContext, Task> CreateHttpClientCaller(MethodInfo method)
        {
            var mr = method.GetReflector();
            var tr = method.DeclaringType.GetReflector();
            var clientName = mr.GetCustomAttributesDistinctBy<ClientNameAttribute>(tr)
                .Select(i => i.Name)
                .FirstOrDefault() ?? Options.DefaultName;
            var clientSetters = mr.GetCustomAttributesDistinctBy<ClientSettingsAttribute>(tr).ToArray();
            var requestSetters = mr.GetCustomAttributesDistinctBy<HttpRequestMessageSettingsAttribute>(tr)
                .OrderBy(i => i.Order)
                .ToArray();
            var option = mr.GetCustomAttributesDistinctBy<HttpCompletionOptionAttribute>(tr)
                .Select(i => i.Option)
                .FirstOrDefault(HttpCompletionOption.ResponseHeadersRead);
            var contentType = mr.GetCustomAttributesDistinctBy<MediaTypeHeaderValueAttribute>(tr)
                .Select(i => i.ContentType)
                .FirstOrDefault(JsonContentTypeAttribute.Json);
            var bodyHandler = CreateBodyHandler(mr.Parameters.FirstOrDefault(i => i.IsDefined<BodyAttribute>())?.MemberInfo, contentType);
            var returnValueHandler = CreateReturnValueHandler(method);
            return async (context) =>
            {
                var handler = lazyClientFactory.GetValue(context);
                var client = handler.CreateClient(clientName);
                foreach (var setter in clientSetters)
                {
                    setter.SetClient(client, context);
                }
                var message = new HttpRequestMessage();
                message.Content = await bodyHandler(context);
                foreach (var setter in requestSetters)
                {
                    setter.SetRequest(message, context);
                }
                var resp = await client.SendAsync(message, option);
                await returnValueHandler(resp.Content, context);
            };
        }

        private Func<HttpContent, AspectContext, Task> CreateReturnValueHandler(MethodInfo method)
        {
            if (method.IsVoid())
            {
                return (content, context) => Task.CompletedTask;
            }
            else if (method.IsReturnValueTask())
            {
                return CreateDeserializeCaller(method.ReturnType.GenericTypeArguments[0], CallDeserializeValueTaskAsync);
            }
            else if (method.IsReturnTask())
            {
                return CreateDeserializeCaller(method.ReturnType.GenericTypeArguments[0], CallDeserializeTaskAsync);
            }
            else if (method.IsValueTask())
            {
                return (content, context) => 
                {
                    context.ReturnValue = TaskUtils.CompletedValueTask;
                    return Task.CompletedTask;
                };
            }
            else if (method.IsTask())
            {
                return (content, context) =>
                {
                    context.ReturnValue = Task.CompletedTask;
                    return Task.CompletedTask;
                };
            }
            else
            {
                return CreateDeserializeCaller(method.ReturnType, CallDeserialize);
            }
        }

        private Func<HttpContent, AspectContext, Task> CreateDeserializeCaller(Type returnType, MethodInfo deserializeMethod)
        {
            var caller = deserializeMethod.MakeGenericMethod(returnType)
                .CreateDelegate<Func<HttpClientInterceptor, HttpContent, AspectContext, Task>>(typeof(Task),
                new Type[] { typeof(HttpClientInterceptor), typeof(HttpContent), typeof(AspectContext) },
                (il) =>
                {
                    il.EmitLoadArg(1);
                    il.EmitLoadArg(2);
                });
            return async (content, context) => await caller(this, content, context);
        }

        public async Task Deserialize<T>(HttpContent content, AspectContext context)
        {
            var result = await lazyClientFactory.GetValue(context).DeserializeAsync<T>(content);
            context.ReturnValue = result;
        }

        public async Task DeserializeTaskAsync<T>(HttpContent content, AspectContext context)
        {
            var result = await lazyClientFactory.GetValue(context).DeserializeAsync<T>(content);
            context.ReturnValue = Task.FromResult(result);
        }

        public async Task DeserializeValueTaskAsync<T>(HttpContent content, AspectContext context)
        {
            var result = await lazyClientFactory.GetValue(context).DeserializeAsync<T>(content);
            context.ReturnValue = new ValueTask<T>(result);
        }

        private Func<AspectContext, Task<HttpContent>> CreateBodyHandler(ParameterInfo parameter, MediaTypeHeaderValue contentType)
        {
            if (parameter == null)
            {
                return c => Task.FromResult<HttpContent>(new StringContent(string.Empty));
            }
            else
            {
                var index = parameter.Position;
                var type = parameter.ParameterType;
                var serializer = SerializeAsync.MakeGenericMethod(type).CreateDelegate<Func<IHttpClientHandler, AspectContext, Task<HttpContent>>>(typeof(Task<HttpContent>),
                    new Type[] { typeof(IHttpClientHandler), typeof(AspectContext), typeof(Task<HttpContent>) },
                    (il) =>
                    {
                        il.EmitLoadArg(1);
                        il.Emit(OpCodes.Call, Constants.GetParameters);
                        il.EmitInt(index);
                        il.Emit(OpCodes.Ldelem_Ref);
                        il.EmitConvertObjectTo(type);
                        il.EmitString(contentType.MediaType);
                    });
                return async c => await serializer(lazyClientFactory.GetValue(c), c);
            }
        }
    }
}