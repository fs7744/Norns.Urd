using System;
using Xunit;

namespace Test.Norns.Urd.Linq
{
    public class DistinctByTest
    {
        [Fact]
        public void T()
        {
            var d = System.Text.Json.JsonSerializer.Serialize(DateTime.Now, new System.Text.Json.JsonSerializerOptions() {  });
            var a = new UriBuilder("WeatherForecast?a=23");
            a.Query = "b=45";
            //Uri.TryCreate(new Uri("http://localhost:5000/WeatherForecast?a=23"), "b=45", out var r);
            a.Uri.ToString();
        }
    }
}