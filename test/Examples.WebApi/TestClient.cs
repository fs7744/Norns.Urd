using Norns.Urd.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Examples.WebApi
{
    //[AcceptXml]
    [BaseAddress("http://localhost.:5000")]
    public interface ITestClient
    {
        //[Cache(nameof(GetWeatherForecastAsync))]
        [Get("WeatherForecast")]
        Task<List<WeatherForecast>> GetWeatherForecastAsync([Query(Alias = "dsd")] DateTime time, [OutResponseHeader("Content-Type")] out string header);

        [Get("WeatherForecast/file")]
        [AcceptOctetStream]
        Task<Stream> DownloadAsync();

        [Post("WeatherForecast/file")]
        [OctetStreamContentType]
        Task UpoladAsync([Body]Stream f);
    }

    public class LoginUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}