# [切换为 中文文档](https://fs7744.github.io/Norns.Urd/zh-cn/index.html)

# Welcome to Norns.Urd

![build](https://github.com/fs7744/Norns.Urd/workflows/build/badge.svg)
[![GitHub](https://img.shields.io/github/license/fs7744/Norns.Urd)](https://github.com/fs7744/Norns.Urd/blob/main/LICENSE)

Norns.urd is a lightweight AOP framework based on emit which do dynamic proxy

The purpose of completing this framework mainly comes from the following personal wishes:

- Static AOP and dynamic AOP are implemented once
- How can an AOP framework only do dynamic proxy but can work with other DI frameworks like `Microsoft.Extensions.DependencyInjection`
- How can an AOP make both sync and Async methods compatible and leave implementation options entirely to the user

Hopefully, this library will be of some useful to you

By the way, if you're not familiar with AOP, check out these articles：

[Aspect-oriented programming](https://en.wikipedia.org/wiki/Aspect-oriented_programming)

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

# Fundamentals

This article provides an overview of key topics for understanding how to develop Norns.Urd.Interceptors

# Some design of Norns.Urd

## Implementation premise of Norns.Urd

1. Support both sync / async method and user can choose sync or async by like

    - The good thing about this is that it's twice as much work. Sync and Async are completely split into two implementations.

    - The Interceptor interface provided to the user needs to provide a solution that combines Sync and Async in one set of implementation code. After all, the user cannot be forced to implement two sets of code. Many scenario users do not need to implement two sets of code for the difference between Sync and Async

2. No DI Implementation，but work fine with DI

    - if the built-in DI container can make support generic scene is very simple, after all, from the DI container instantiation objects must have a definite type, but, now there are so many implementation library, I don't want to realize many functions for some scenarios (I really lazy, or the library can't write that long)

    - but DI container does decoupling is very good, I often benefit and decrease a lot of code changes, so do a aop libraries must be considered based on the DI container do support, in this case, DI support open generic/custom instantiation method to do support, DI and aop inside have to provide user call method, otherwise it doesn't work out (so calculate down, I really lazy? Am I digging a hole for myself?)

## How to resolve these problem？ 

The current solution is not necessarily perfect, but it has solved the problem temporarily (please tell me if there is a better solution, I urgently need to learn).

### What interceptor writing patterns are provided to the user?

I have encountered some other AOP implementation frameworks in the past, many of which require the intercepting code to be divided into method before/method after/with exceptions, etc. Personally, I think this form affects the code thinking of the interceptor implementation to some extent, and I always feel that it is not smooth enough

But like [ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-5.0)feels pretty good, as shown in the following figure and code:

![https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/index/_static/request-delegate-pipeline.png?view=aspnetcore-5.0](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/index/_static/request-delegate-pipeline.png?view=aspnetcore-5.0)

``` csharp
app.Run(async context =>
{
    await context.Response.WriteAsync("Hello, World!");
});
```

The interceptor should also be able to do this, so the interceptor code should look like this:

``` csharp
public class ConsoleInterceptor 
{
    public async Task InvokeAsync(Context context, Delegate next)
    {
        Console.WriteLine("Hello, World!");
        await next(context);
    }
}
```

### How do the Sync and Async methods split up? How can they be combined? How do users choose to implement sync or Async or both?

``` csharp

public delegate Task AsyncAspectDelegate(AspectContext context);

public delegate void AspectDelegate(AspectContext context);

// resolution:
// Create two sets of call chains that make a complete differentiating between Sync and Async by AspectDelegate and AsyncAspectDelegate, depending on the intercepted method itself

public abstract class AbstractInterceptor : IInterceptor
{
    public virtual void Invoke(AspectContext context, AspectDelegate next)
    {
        InvokeAsync(context, c =>
        {
            next(c);
            return Task.CompletedTask;
        }).ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
    }

// merge:
// Implements transformation method content by default so that various interceptors can be mixed into a call chain for Middleware

    public abstract Task InvokeAsync(AspectContext context, AsyncAspectDelegate next);

// User autonomous selection:
// Providing both the Sync and Async interceptor methods can be overloaded and the user can choose
// So the user can call special non-asynchronous optimization code in Async, and needless to say awit in Sync will affect performance.
// If you think it affects performance, you can reload yourself if you care. If you don't care, you can choose
}
```

### No DI and how to support other DI ？

DI framework has registration type, we can use emit to generate proxy class, replace the original registration, can achieve compatibility.

Of course, each DI framework requires some custom implementation code to support (alas, workload again)

### How to support `AddTransient<IMTest>(x => new NMTest())`？

Due to the usage of this DI framework, it is not possible to get the actual type to be used through the Func function. Instead, it can only generate the bridge proxy type through the emit according to the definition of IMTest. The pseudo-code looks like the following:

``` csharp

interface IMTest
{
    int Get(int i);
}

class IMTestProxy : IMTest
{
    IMTest instance = (x => new NMTest())();

    int Get(int i) => instance.Get(i);
}

```

### How to support `.AddTransient(typeof(IGenericTest<,>), typeof(GenericTest<,>))` ？

The only difficulty is that it is not easy to generate method calls such as `Get<T>()`, because IL needs to reflect the specific methods found, such as `Get<int>()`,`Get<bool>()`, etc., it cannot be ambiguous `Get<T>()`.

The only way to solve this problem is to defer the actual invocation until the runtime invocation is regenerated into a specific invocation. The pseudo-code is roughly as follows:

``` csharp

interface GenericTest<T,R>
{
    T Get<T>(T i) => i;
}

class GenericTestProxy<T,R> : GenericTest<T,R>
{
    T Get<T>(T i) => this.GetType().GetMethod("Get<T>").Invoke(i);
}

```

# Nuget Packages

| Package Name |  NuGet | Downloads  |
|--------------|  ------- |  ----  |
| Norns.Urd | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd)](https://www.nuget.org/packages/Norns.Urd/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd) |
| Norns.Urd.Extensions.DependencyInjection | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd.Extensions.DependencyInjection)](https://www.nuget.org/packages/Norns.Urd.Extensions.DependencyInjection/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd.Extensions.DependencyInjection) |