using Norns.Urd.Caching;
using Norns.Urd.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Examples.WebApi
{
    [AcceptXml]
    [BaseAddress("http://localhost:5000")]
    public interface ITestClient
    {
        //[Cache(nameof(GetWeatherForecastAsync))]
        [Get("WeatherForecast")]
        Task<List<WeatherForecast>> GetWeatherForecastAsync();
    }
}