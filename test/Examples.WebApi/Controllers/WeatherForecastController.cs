using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Examples.WebApi.Controllers
{
    public interface IAopTest
    {
        IEnumerable<WeatherForecast> Get();
    }

    public class AopTest : IAopTest
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        public virtual IEnumerable<WeatherForecast> Get()
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
        public IEnumerable<WeatherForecast> Get() => test.Get();
    }
}