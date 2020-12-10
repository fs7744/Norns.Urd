#### [切换为 中文文档](https://fs7744.github.io/Norns.Urd/zh-cn/index.html)
# Contents
- [Welcome to Norns.Urd](#welcome-to-nornsurd)
- [Simple Benchmark](#simple-benchmark)
- [Quick start](#quick-start)
- [Fundamentals](#fundamentals)
    - [Interceptor](#interceptor)
        - [Interceptor structure definition](#interceptor-structure-definition)
        - [Interceptor junction type](#interceptor-junction-type)
        - [Global interceptors vs. display interceptors](#global-interceptors-vs-display-interceptors)
        - [Interceptor filter mode](#interceptor-filter-mode)
        - [Aop limit](#aop-limit)
    - [The default implementation of Interface and Abstract Class](#the-default-implementation-of-interface-and-abstract-class)
        - [default implementation limit](#default-implementation-limit)
    - [InjectAttribute](#injectattribute)
    - [Polly](#polly)
        - [TimeoutAttribute](#timeoutattribute)
        - [RetryAttribute](#retryattribute)
- [Some design of Norns.Urd](#some-design-of-nornsurd)
- [Nuget Packages](#nuget-packages)

# Welcome to Norns.Urd

![build](https://github.com/fs7744/Norns.Urd/workflows/build/badge.svg)
[![GitHub](https://img.shields.io/github/license/fs7744/Norns.Urd)](https://github.com/fs7744/Norns.Urd/blob/main/LICENSE)
[![GitHub Repo stars](https://img.shields.io/github/stars/fs7744/Norns.Urd?style=social)](https://github.com/fs7744/Norns.Urd)

Norns.urd is a lightweight AOP framework based on emit which do dynamic proxy.

It base on netstandard2.0.

The purpose of completing this framework mainly comes from the following personal wishes:

- Static AOP and dynamic AOP are implemented once
- How can an AOP framework only do dynamic proxy but can work with other DI frameworks like `Microsoft.Extensions.DependencyInjection`
- How can an AOP make both sync and Async methods compatible and leave implementation options entirely to the user

Hopefully, this library will be of some useful to you

By the way, if you're not familiar with AOP, check out these articles：

[Aspect-oriented programming](https://en.wikipedia.org/wiki/Aspect-oriented_programming)

# Simple Benchmark

Just simple benchmark test, and does not represent the whole scenario

Castle and AspectCore are excellent libraries,

Many implementations of Norns.urd refer to the source code of Castle and AspectCore.

<pre><code>
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1198 (1909/November2018Update/19H2)
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.100
  [Host]     : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT
  DefaultJob : .NET Core 5.0.0 (CoreCLR 5.0.20.51904, CoreFX 5.0.20.51904), X64 RyuJIT
</code></pre>
<pre><code></code></pre>

<table>
<thead><tr><th>                                  Method</th><th>Mean</th><th>Error</th><th>StdDev</th><th>Median</th><th>Gen 0</th><th>Gen 1</th><th>Gen 2</th><th>Allocated</th>
</tr>
</thead><tbody><tr><td>TransientInstanceCallSyncMethodWhenNoAop</td><td>69.10 ns</td><td>1.393 ns</td><td>2.512 ns</td><td>69.70 ns</td><td>0.0178</td><td>-</td><td>-</td><td>112 B</td>
</tr><tr><td>TransientInstanceCallSyncMethodWhenNornsUrd</td><td>148.38 ns</td><td>2.975 ns</td><td>5.588 ns</td><td>145.76 ns</td><td>0.0534</td><td>-</td><td>-</td><td>336 B</td>
</tr><tr><td>TransientInstanceCallSyncMethodWhenCastle</td><td>222.48 ns</td><td>0.399 ns</td><td>0.312 ns</td><td>222.50 ns</td><td>0.0815</td><td>-</td><td>-</td><td>512 B</td>
</tr><tr><td>TransientInstanceCallSyncMethodWhenAspectCore</td><td>576.04 ns</td><td>7.132 ns</td><td>10.229 ns</td><td>573.46 ns</td><td>0.1030</td><td>-</td><td>-</td><td>648 B</td>
</tr><tr><td>TransientInstanceCallAsyncMethodWhenNoAop</td><td>114.61 ns</td><td>0.597 ns</td><td>0.499 ns</td><td>114.58 ns</td><td>0.0408</td><td>-</td><td>-</td><td>256 B</td>
</tr><tr><td>TransientInstanceCallAsyncMethodWhenNornsUrd</td><td>206.36 ns</td><td>0.937 ns</td><td>0.830 ns</td><td>206.18 ns</td><td>0.0763</td><td>-</td><td>-</td><td>480 B</td>
</tr><tr><td>TransientInstanceCallAsyncMethodWhenCastle</td><td>250.98 ns</td><td>3.315 ns</td><td>3.101 ns</td><td>252.16 ns</td><td>0.1044</td><td>-</td><td>-</td><td>656 B</td>
</tr><tr><td>TransientInstanceCallAsyncMethodWhenAspectCore</td><td>576.00 ns</td><td>4.160 ns</td><td>3.891 ns</td><td>574.99 ns</td><td>0.1373</td><td>-</td><td>-</td><td>864 B</td>
</tr></tbody></table>

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

## Interceptor 

In Norns.urd, Interceptor Interceptor is the core of the logic that a user can insert into a method.

### Interceptor structure definition

The interceptor defines the standard structure as `IInterceptor`

``` csharp
public interface IInterceptor
{
    // Users can customize the interceptor Order with Order, sorted by ASC, in which both the global interceptor and the display interceptor are included
    int Order { get; }

    // Synchronous interception method
    void Invoke(AspectContext context, AspectDelegate next);

    // Asynchronous interception method
    Task InvokeAsync(AspectContext context, AsyncAspectDelegate next);

    // You can set how the interceptor chooses whether to filter or not to intercept a method, in addition to the NonAspectAttribute and global NonPredicates that can influence filtering
    bool CanAspect(MethodInfo method);
}
```

### Interceptor junction type

Interceptors from actual design only ` IInterceptor ` that a unified definition, but due to the single inheritance and ` csharp Attribute ` language limitation, so have a ` AbstractInterceptorAttribute ` and ` AbstractInterceptor ` two classes.

#### AbstractInterceptorAttribute （Display interceptor）

``` csharp 
public abstract class AbstractInterceptorAttribute : Attribute, IInterceptor
{
    public virtual int Order { get; set; }

    public virtual bool CanAspect(MethodInfo method) => true;

    // If the user wants to reduce the performance penalty of converting an asynchronous method to a synchronous call in a synchronous interceptor method by default, he can choose to overload the implementation.
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

    // The default is to implement only the asynchronous interceptor method
    public abstract Task InvokeAsync(AspectContext context, AsyncAspectDelegate next);
}
```

An example of an interceptor implementation：

``` csharp 
public class AddTenInterceptorAttribute : AbstractInterceptorAttribute
{
    public override void Invoke(AspectContext context, AspectDelegate next)
    {
        next(context);
        AddTen(context);
    }

    private static void AddTen(AspectContext context)
    {
        if (context.ReturnValue is int i)
        {
            context.ReturnValue = i + 10;
        }
        else if(context.ReturnValue is double d)
        {
            context.ReturnValue = d + 10.0;
        }
    }

    public override async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
    {
        await next(context);
        AddTen(context);
    }
}
```

##### `InterceptorAttribute` Interceptor usage

- interface / class / method You can set the`Attribute`，like

``` csharp 
[AddTenInterceptor]
public interface IGenericTest<T, R> : IDisposable
{
    // or
    //[AddTenInterceptor]
    T GetT();
}
```

- It can also be set in the global interceptor

``` csharp 
public void ConfigureServices(IServiceCollection services)
{
    services.ConfigureAop(i => i.GlobalInterceptors.Add(new AddTenInterceptorAttribute()));
}
```

#### AbstractInterceptor

And ` AbstractInterceptorAttribute ` almost identical, but not a ` Attribute `, cannot be used for corresponding scene, only in the use of the interceptor. In itself, it is provided for a user to create an Interceptor that does not want to simplify the 'Attribute' scenario.

##### `Interceptor`Interceptor usage

Can only be set in a global interceptor

``` csharp 
public void ConfigureServices(IServiceCollection services)
{
    services.ConfigureAop(i => i.GlobalInterceptors.Add(new AddSixInterceptor()));
}
```

### Global interceptors vs. display interceptors

- A global interceptor is a method that intercepts all proxying methods. It only needs to be declared once and is valid globally

``` csharp 
public void ConfigureServices(IServiceCollection services)
{
    services.ConfigureAop(i => i.GlobalInterceptors.Add(new AddSixInterceptor()));
}
```

- Display interceptor must use ` AbstractInterceptorAttribute ` in all places need to display statement

``` csharp 
[AddTenInterceptor]
public interface IGenericTest<T, R> : IDisposable
{
    // or
    //[AddTenInterceptor]
    T GetT();
}
```

So just use what the user thinks is convenient

### Interceptor filter mode

Norns.Urd Provide the following three filtering methods

- Global filtering

``` csharp 
services.ConfigureAop(i => i.NonPredicates.AddNamespace("Norns")
    .AddNamespace("Norns.*")
    .AddNamespace("System")
    .AddNamespace("System.*")
    .AddNamespace("Microsoft.*")
    .AddNamespace("Microsoft.Owin.*")
    .AddMethod("Microsoft.*", "*"));
```

- According to filter

``` csharp 
[NonAspect]
public interface IGenericTest<T, R> : IDisposable
{
}
```

- The interceptor itself filters

``` csharp 
public class ParameterInjectInterceptor : AbstractInterceptor
{
    public override bool CanAspect(MethodInfo method)
    {
        return method.GetReflector().Parameters.Any(i => i.IsDefined<InjectAttribute>());
    }
}
```

### Aop limit

- When service type is class, only virtual and subclasses have access to methods that can be proxy intercepted
- When which type's mehtod has parameter is in readonly struct can't proxy

## The default implementation of Interface and Abstract Class

Norns.urd implements the default subtype if you register with the DI framework no actual implementation of 'Interface' and 'Abstract Class'.

Why is this feature available?

This is to provide some low-level implementation support for the idea of declarative coding, so that more students can customize some of their own declarative libraries and simplify the code, such as implementing a declarative HttpClient

### default implementation limit

- Property injection is not supported
- The default implementation generated by Norns.urd is the default value of the return type

### demo

We will complete a simple httpClient as an example. Here is a brief demo

1. If adding 10 was our logic like an HTTP call, we could put all the add 10 logic in the interceptor

``` csharp 
public class AddTenAttribute : AbstractInterceptorAttribute
{
    public override void Invoke(AspectContext context, AspectDelegate next)
    {
        next(context);
        AddTen(context);
    }

    private static void AddTen(AspectContext context)
    {
        if (context.ReturnValue is int i)
        {
            context.ReturnValue = i + 10;
        }
        else if(context.ReturnValue is double d)
        {
            context.ReturnValue = d + 10.0;
        }
    }

    public override async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
    {
        await next(context);
        AddTen(context);
    }
}
```

2. Define declarate client

``` csharp 
[AddTen]
public interface IAddTest
{
    int AddTen();

    // The default implementation in the interface is not replaced by norns.urd, which provides some scenarios where users can customize the implementation logic
    public int NoAdd() => 3;
}
```

3. Registered client

``` csharp 
services.AddTransient<IAddTest>();
services.ConfigureAop();
```

4. Use it

``` csharp 
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        IAddTest a;
        public WeatherForecastController(IAddTest b)
        {
            a = b;
        }

        [HttpGet]
        public int GetAddTen() => a.AddTen();
    }
```

## InjectAttribute

`InjectAttribute` Is a functional complement to the default implementation of Interface and Abstract Class,

Especially when you're doing declarative clients and things like that, and you're providing custom Settings, like the interface default interface implementation,

The user may need to get an instance from DI, so there are two ways to supplement it.

### ParameterInject

Method parameters can be set as `InjectAttribute`:
- When the parameter is null, an attempt is made to get the instance from DI
- When the parameter is not null, the pass value will not be overridden and the pass parameter value will remain

Example:

``` csharp
public interface IInjectTest
{
    public ParameterInjectTest T([Inject] ParameterInjectTest t = null) => t;
}
```

### PropertyInject

``` csharp
public interface IInjectTest
{
    [Inject]
    ParameterInjectInterceptorTest PT { get; set; }
}
```

### FieldInject

According to industry coding conventions, FIELD is not recommended to use without assignment, so this feature can lead to code review problems that need to be fixed

``` csharp
public class ParameterInjectTest : IInjectTest
{
    [Inject]
    ParameterInjectInterceptorTest ft;
}
```

## Polly

Polly is .NET resilience and transient-fault-handling library.

这里通过Norns.Urd将Polly的各种功能集成为更加方便使用的功能

### 如何启用 Norns.Urd + Polly, 只需使用`EnablePolly()`

如：

``` csharp
new ServiceCollection()
    .AddTransient<DoTimeoutTest>()
    .ConfigureAop(i => i.EnablePolly())
```

### TimeoutAttribute

``` csharp
[Timeout(1)]  // timeout 1 seconds, when timeout will throw TimeoutRejectedException
double Wait(double seconds);

[Timeout("00:00:00.100")]  // timeout 100 milliseconds, only work on async method when no CancellationToken
async Task<double> WaitAsync(double seconds, CancellationToken cancellationToken = default);

[Timeout("00:00:01")]  // timeout 1 seconds, but no work on async method when no CancellationToken
async Task<double> NoCancellationTokenWaitAsync(double seconds);
```

## RetryAttribute

``` csharp
[Retry(2, ExceptionType = typeof(AccessViolationException))]  // retry 2 times when if throw Exception
void Do()
```

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
| Norns.Urd.Extensions.Polly | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd.Extensions.Polly)](https://www.nuget.org/packages/Norns.Urd.Extensions.Polly/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd.Extensions.Polly) |