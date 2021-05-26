using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Norns.Urd.Caching;

namespace Examples.WebApi.Controllers
{
    public interface IAopTest
    {
        [Cache(nameof(Get), AbsoluteExpirationRelativeToNow = "00:00:05")]
        IEnumerable<WeatherForecast> Get();
    }

    public class AopTest : IAopTest, IDisposable
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        public void Dispose()
        {
        }

        public IEnumerable<WeatherForecast> Get()
        {
            var rng = new Random();
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }

    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IAopTest test;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IAopTest test)
        {
            _logger = logger;
            this.test = test;
        }

        [HttpGet]
        public virtual IEnumerable<WeatherForecast> Get(DateTime dsd) => test.Get();

        [HttpPost("file")]
        public async Task TestUploadStream()
        {
            using var reader = new StreamReader(HttpContext.Request.Body);
            _logger.LogWarning(await reader.ReadToEndAsync());
        }

        [HttpGet("file")]
        public async Task TestDownloadStream()
        {
            HttpContext.Response.ContentType = "application/octet-stream";
            var bytes = System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString());
            await HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            await HttpContext.Response.CompleteAsync();
        }
    }
}