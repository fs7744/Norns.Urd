# Norns.Urd

![build](https://github.com/fs7744/Norns.Urd/workflows/build/badge.svg)
[![GitHub](https://img.shields.io/github/license/fs7744/Norns.Urd)](https://github.com/fs7744/Norns.Urd/blob/main/LICENSE)
[![GitHub Repo stars](https://img.shields.io/github/stars/fs7744/Norns.Urd?style=social)](https://github.com/fs7744/Norns.Urd)

## Nuget Packages

| Package Name |  NuGet | Downloads  |
|--------------|  ------- |  ----  |
| Norns.Urd | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd?style=flat-square)](https://www.nuget.org/packages/Norns.Urd/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd?style=flat-square) |
| Norns.Urd.Extensions.Polly | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd.Extensions.Polly?style=flat-square)](https://www.nuget.org/packages/Norns.Urd.Extensions.Polly/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd.Extensions.Polly?style=flat-square) |
| Norns.Urd.Caching.Abstractions | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd.Caching.Abstractions?style=flat-square)](https://www.nuget.org/packages/Norns.Urd.Caching.Abstractions/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd.Caching.Abstractions?style=flat-square) |
| Norns.Urd.Caching.Memory | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd.Caching.Memory?style=flat-square)](https://www.nuget.org/packages/Norns.Urd.Caching.Memory/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd.Caching.Memory?style=flat-square) |
| Norns.Urd.Caching.DistributedCache | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd.Caching.DistributedCache?style=flat-square)](https://www.nuget.org/packages/Norns.Urd.Caching.DistributedCache/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd.Caching.DistributedCache?style=flat-square) |
| Norns.Urd.HttpClient | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd.HttpClient?style=flat-square)](https://www.nuget.org/packages/Norns.Urd.HttpClient/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd.HttpClient?style=flat-square) |
| Norns.Urd.HttpClient.NewtonsoftJson | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd.HttpClient.NewtonsoftJson?style=flat-square)](https://www.nuget.org/packages/Norns.Urd.HttpClient.NewtonsoftJson/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd.HttpClient.NewtonsoftJson?style=flat-square) |

## Welcome to Norns.Urd

Norns.urd is a lightweight AOP framework based on emit which do dynamic proxy.

It base on netstandard2.0.

The purpose of completing this framework mainly comes from the following personal wishes:

- Static AOP and dynamic AOP are implemented once
- How can an AOP framework only do dynamic proxy but can work with other DI frameworks like `Microsoft.Extensions.DependencyInjection`
- How can an AOP make both sync and Async methods compatible and leave implementation options entirely to the user

Hopefully, this library will be of some useful to you

## Fundamentals

- Interceptor
  - Attribute Interceptor
  - Global interceptor
- Generate default implementation of Interface and Abstract Class
- InjectAttribute
  - PropertyInject
  - ParameterInject
  - FieldInject
- FallbackAttribute
- Polly
    - TimeoutAttribute
    - RetryAttribute
    - CircuitBreakerAttribute
    - BulkheadAttribute
- CacheAttribute   (support Multistage cache with memory cache and distributed cache)

## How to use

### Quick start

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

### Deep Document

[中文文档](https://fs7744.github.io/Norns.Urd/zh-cn/index.html) |  [Document](https://fs7744.github.io/Norns.Urd/index.html)

## Roadmap

- [x] AOP
- [x] Polly
- [x] Cache
- [x] HttpClient
- [ ] Trace
- [ ] Runtime Metrics collect

## Simple Benchmark

Just simple benchmark test, and does not represent the whole scenario

Castle and AspectCore are excellent libraries,

Many implementations of Norns.urd refer to the source code of Castle and AspectCore.

``` ini

BenchmarkDotNet=v0.13.2, OS=Windows 10 (10.0.19044.1288/21H2/November2021Update)
Intel Core i7-10700 CPU 2.90GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK=6.0.402
  [Host]     : .NET 6.0.10 (6.0.1022.47605), X64 RyuJIT AVX2
  DefaultJob : .NET 6.0.10 (6.0.1022.47605), X64 RyuJIT AVX2


|                                         Method |      Mean |    Error |   StdDev |   Gen0 | Allocated |
|----------------------------------------------- |----------:|---------:|---------:|-------:|----------:|
|       TransientInstanceCallSyncMethodWhenNoAop |  52.41 ns | 0.319 ns | 0.298 ns | 0.0134 |     112 B |
|    TransientInstanceCallSyncMethodWhenNornsUrd | 120.26 ns | 0.237 ns | 0.185 ns | 0.0410 |     344 B |
|      TransientInstanceCallSyncMethodWhenCastle | 168.58 ns | 0.842 ns | 0.788 ns | 0.0610 |     512 B |
|  TransientInstanceCallSyncMethodWhenAspectCore | 439.43 ns | 1.466 ns | 1.225 ns | 0.0772 |     648 B |
|      TransientInstanceCallAsyncMethodWhenNoAop |  88.63 ns | 0.318 ns | 0.282 ns | 0.0305 |     256 B |
|   TransientInstanceCallAsyncMethodWhenNornsUrd | 211.48 ns | 1.315 ns | 1.098 ns | 0.0668 |     560 B |
|     TransientInstanceCallAsyncMethodWhenCastle | 197.95 ns | 0.657 ns | 0.549 ns | 0.0782 |     656 B |
| TransientInstanceCallAsyncMethodWhenAspectCore | 522.63 ns | 4.313 ns | 4.034 ns | 0.1030 |     864 B |

## License
[MIT](https://github.com/fs7744/Norns.Urd/blob/main/LICENSE)
