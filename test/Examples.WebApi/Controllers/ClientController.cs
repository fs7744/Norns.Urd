using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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
            var r = client.GetWeatherForecastAsync();
            return await r;
        }
    }
}