using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Examples.WebApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly ITestClient client;

        public ClientController(ITestClient client)
        {
            this.client = client;
        }

        [HttpGet]
        public async Task<object> GetWeatherForecastAsync()
        {
            var r = client.GetWeatherForecastAsync(DateTime.Now, out string h);
            return await r;
        }

        [HttpGet("download")]
        public async Task<object> DownloadAsync()
        {
            using var r = new StreamReader( await client.DownloadAsync());
            return await r.ReadToEndAsync();
        }

        [HttpGet("upload")]
        public async Task UploadAsync(string str)
        {
            var s = new MemoryStream();
            using var r = new StreamWriter(s);
            await r.WriteAsync(str);
            await r.FlushAsync();
            s.Seek(0, SeekOrigin.Begin);
            await client.UpoladAsync(s);
        }
    }
}