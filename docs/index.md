# [切换为 中文文档](https://fs7744.github.io/Norns.Urd/zh-cn/index.html)

# Welcome to Norns.Urd

Norns.urd is a lightweight AOP framework based on emit which do dynamic proxy

The purpose of completing this framework mainly comes from the following personal wishes:

- Static AOP and dynamic AOP are implemented once
- How can an AOP framework only do dynamic proxy but can work with other DI frameworks like `Microsoft.Extensions.DependencyInjection`
- How can an AOP make both sync and Async methods compatible and leave implementation options entirely to the user

Hopefully, this library will be of some useful to you

# Quick start

This is simple demo whch to do global interceptor, full code fot the demo you can see [Examples.WebApi](https://github.com/fs7744/Norns.Urd/tree/main/test/Examples.WebApi)

1. create ConsoleInterceptor.cs

    ```csharp
    using Norns.Urd;
    using Norns.Urd.Reflection;
    using System;
    using System.Threading.Tasks;

    namespace Examples.WebApi
    {
        public class ConsoleInterceptor : AbstractInterceptor
        {
            public override async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
            {
                Console.WriteLine($"{context.Service.GetType().GetReflector().FullDisplayName}.{context.Method.GetReflector().DisplayName}");
                await next(context);
            }
        }
    }
    ```

2. set WeatherForecastController's method be virtual

    ```csharp
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        [HttpGet]
        public virtual IEnumerable<WeatherForecast> Get() => test.Get();
    }
    ```

3. AddControllersAsServices

    ```csharp
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddControllersAsServices();
    }
    ```

4. add GlobalInterceptor to di

    ```csharp
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddControllersAsServices();
        services.ConfigureAop(i => i.GlobalInterceptors.Add(new ConsoleInterceptor()));
    }
    ```

5. run 

    you will see this in console 

    ``` shell
    Norns.Urd.DynamicProxy.Generated.WeatherForecastController_Proxy_Inherit.IEnumerable<WeatherForecast> Get()
    ```