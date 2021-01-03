using Microsoft.Extensions.DependencyInjection;
using Moq;
using Norns.Urd.Http;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Test.Norns.Urd.HttpClients
{
    public class InterfaceClientTest
    {
        public class Data
        {
            public int MyProperty { get; set; }
        }

        [BaseAddress("http://www.test.com/test/")]
        [ClientName("Test")]
        public interface IHttpClient
        {
            [Get("go/{id}")]
            HttpResponseMessage GetHttpResponseMessage([Route] string id);

            [Post("go{id}/{id}/{id}/{id}")]
            Task<HttpResponseMessage> PostHttpResponseMessage([Route] string id, [Query(Alias = "a")]double d, [Body]Data data);
        }

        public class TestHttpMessageHandler : HttpMessageHandler
        {
            private readonly HttpStatusCode statusCode;
            private readonly string method;

            public TestHttpMessageHandler(HttpStatusCode statusCode, string method)
            {
                this.statusCode = statusCode;
                this.method = method;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var resp = new HttpResponseMessage();
                if (request.Method.Method == method)
                {
                    resp.StatusCode = statusCode;
                    resp.Headers.TryAddWithoutValidation("url", request.RequestUri.ToString());
                    resp.Content = request.Content;
                    foreach (var item in request.Headers)
                    {
                        resp.Headers.Add(item.Key, item.Value);
                    }
                }
                return Task.FromResult(resp);
            }
        }

        private IHttpClient GetMockClient(HttpStatusCode statusCode, string method, string clientName = "Test")
        {
            var httpClientFactory = new Mock<IHttpClientFactory>();
            httpClientFactory.Setup(i => i.CreateClient(clientName))
                .Returns(new HttpClient(new TestHttpMessageHandler(statusCode, method)));
            return new ServiceCollection()
                .AddTransient<IHttpClient>()
                .ConfigureAop(i => i.EnableHttpClient())
                .AddSingleton<IHttpClientFactory>(httpClientFactory.Object)
                .BuildServiceProvider()
               .GetRequiredService<IHttpClient>();
        }

        [Fact]
        public void GetWhenReturnHttpResponseMessage()
        {
            var client = GetMockClient(HttpStatusCode.Accepted, "GET");
            var resp = client.GetHttpResponseMessage("33");
            Assert.Equal("http://www.test.com/test/go/33", resp.Headers.GetValues("url").First());
            Assert.IsType<StringContent>(resp.Content);
        }

        [Fact]
        public async Task PostWhenReturnHttpResponseMessage()
        {
            var client = GetMockClient(HttpStatusCode.Accepted, "POST");
            var resp = await client.PostHttpResponseMessage("33", 66.6, new Data() { MyProperty = 5 });
            Assert.Equal("http://www.test.com/test/go33/33/33/33?a=66.6", resp.Headers.GetValues("url").First());
            Assert.IsType<StreamContent>(resp.Content);
            Assert.Equal("application/json; charset=utf-8", resp.Content.Headers.ContentType.ToString());
        }
    }
}