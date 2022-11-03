#### [See English](https://fs7744.github.io/Norns.Urd/index.html)
# 目录
- [欢迎了解 Norns.Urd](#欢迎了解-nornsurd)
- [Simple Benchmark](#simple-benchmark)
- [快速入门指南](#快速入门指南)
- [功能说明](#功能说明)
    - [Interceptor 拦截器](#Interceptor-拦截器)
        - [拦截器结构定义](#拦截器结构定义)
        - [拦截器结类型](#拦截器结类型)
        - [全局拦截器 vs 显示拦截器](#全局拦截器-vs-显示拦截器)
        - [拦截器的过滤方式](#拦截器的过滤方式)
        - [AOP限制](#aop限制)
    - [Interface和Abstract Class的默认实现](#interface和abstract-class的默认实现)
        - [默认实现限制](#默认实现限制)
    - [InjectAttribute](#injectattribute)
    - [FallbackAttribute](#fallbackattribute)
    - [Polly](#polly)
        - [TimeoutAttribute](#timeoutattribute)
        - [RetryAttribute](#retryattribute)
        - [CircuitBreakerAttribute](#circuitbreakerattribute)
        - [BulkheadAttribute](#bulkheadattribute)
    - [CacheAttribute](#cacheattribute)
    - [HttpClient](#httpclient)
- [Norns.Urd 中的一些设计](#nornsurd-中的一些设计)

# 欢迎了解 Norns.Urd

![build](https://github.com/fs7744/Norns.Urd/workflows/build/badge.svg)
[![GitHub](https://img.shields.io/github/license/fs7744/Norns.Urd)](https://github.com/fs7744/Norns.Urd/blob/main/LICENSE)
[![GitHub Repo stars](https://img.shields.io/github/stars/fs7744/Norns.Urd?style=social)](https://github.com/fs7744/Norns.Urd)

Norns.Urd 是一个基于emit实现动态代理的轻量级AOP框架.

版本基于 netstandard2.0. 所以哪些.net 版本能用你懂的。

完成这个框架的目的主要出自于个人以下意愿：

- 静态AOP和动态AOP都实现一次
- 如果不实现DI，怎么将AOP框架实现与其他现有DI框架集成
- 一个AOP 如何将 sync 和 async 方法同时兼容且如何将实现选择权完全交予用户

希望该库能对大家有些小小的作用

对了，如果不了解AOP的同学，可以看看这些文章：

[面向切面的程序设计](https://zh.wikipedia.org/wiki/%E9%9D%A2%E5%90%91%E5%88%87%E9%9D%A2%E7%9A%84%E7%A8%8B%E5%BA%8F%E8%AE%BE%E8%AE%A1)

[什么是面向切面编程AOP？](https://www.zhihu.com/question/24863332)
    
[AOP 有几种实现方式？](https://xie.infoq.cn/article/6f65df715a020c0b77f4cd266)

# Simple Benchmark

只是一个简单性能测试，不代表全部场景，也没有故意对比，

Castle 和 AspectCore 都是非常优秀的库，

Norns.Urd 很多实现都是参考了Castle 和 AspectCore的源码的。

<pre><code>
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.18363.1256 (1909/November2018Update/19H2)
Intel Core i7-9750H CPU 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET Core SDK=5.0.101
  [Host]     : .NET Core 5.0.1 (CoreCLR 5.0.120.57516, CoreFX 5.0.120.57516), X64 RyuJIT
  DefaultJob : .NET Core 5.0.1 (CoreCLR 5.0.120.57516, CoreFX 5.0.120.57516), X64 RyuJIT
</code></pre>
<pre><code></code></pre>

<table>
<thead><tr><th>                                  Method</th><th>Mean</th><th>Error</th><th>StdDev</th><th>Gen 0</th><th>Gen 1</th><th>Gen 2</th><th>Allocated</th>
</tr>
</thead><tbody><tr><td>TransientInstanceCallSyncMethodWhenNoAop</td><td>61.77 ns</td><td>0.311 ns</td><td>0.291 ns</td><td>0.0178</td><td>-</td><td>-</td><td>112 B</td>
</tr><tr><td>TransientInstanceCallSyncMethodWhenNornsUrd</td><td>155.58 ns</td><td>1.038 ns</td><td>0.971 ns</td><td>0.0548</td><td>-</td><td>-</td><td>344 B</td>
</tr><tr><td>TransientInstanceCallSyncMethodWhenCastle</td><td>213.94 ns</td><td>1.213 ns</td><td>1.076 ns</td><td>0.0815</td><td>-</td><td>-</td><td>512 B</td>
</tr><tr><td>TransientInstanceCallSyncMethodWhenAspectCore</td><td>508.71 ns</td><td>2.334 ns</td><td>2.183 ns</td><td>0.1030</td><td>-</td><td>-</td><td>648 B</td>
</tr><tr><td>TransientInstanceCallAsyncMethodWhenNoAop</td><td>92.58 ns</td><td>0.793 ns</td><td>0.619 ns</td><td>0.0408</td><td>-</td><td>-</td><td>256 B</td>
</tr><tr><td>TransientInstanceCallAsyncMethodWhenNornsUrd</td><td>242.98 ns</td><td>0.818 ns</td><td>0.765 ns</td><td>0.0892</td><td>-</td><td>-</td><td>560 B</td>
</tr><tr><td>TransientInstanceCallAsyncMethodWhenCastle</td><td>225.98 ns</td><td>0.521 ns</td><td>0.487 ns</td><td>0.1044</td><td>-</td><td>-</td><td>656 B</td>
</tr><tr><td>TransientInstanceCallAsyncMethodWhenAspectCore</td><td>565.25 ns</td><td>2.377 ns</td><td>2.107 ns</td><td>0.1373</td><td>-</td><td>-</td><td>864 B</td>
</tr></tbody></table>

# 快速入门指南

这是一个简单的全局AOP拦截的简单示例，具体详细示例代码可以参阅[Examples.WebApi](https://github.com/fs7744/Norns.Urd/tree/main/test/Examples.WebApi)

1. 创建 ConsoleInterceptor.cs

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

2. 设置 WeatherForecastController 的方法为 virtual

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

4. 设置di 容器启用aop 功能

    ```csharp
    // This method gets called by the runtime. Use this method to add services to the container.
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddControllersAsServices();
        services.ConfigureAop(i => i.GlobalInterceptors.Add(new ConsoleInterceptor()));
    }
    ```

5. 运行程序

    你会在控制台看见如下输出

    ``` shell
    Norns.Urd.DynamicProxy.Generated.WeatherForecastController_Proxy_Inherit.IEnumerable<WeatherForecast> Get()
    ```

# 功能说明

## Interceptor 拦截器

在Norns.Urd中，Interceptor 拦截器是用户可以在方法插入自己的逻辑的核心。

### 拦截器结构定义

拦截器定义了标准结构为`IInterceptor`

``` csharp
public interface IInterceptor
{
    // 用户可以通过Order自定义拦截器顺序，排序方式为ASC，全局拦截器和显示拦截器都会列入排序中
    int Order { get; }

    // 同步拦截方法
    void Invoke(AspectContext context, AspectDelegate next);

    // 异步拦截方法
    Task InvokeAsync(AspectContext context, AsyncAspectDelegate next);

    // 可以设置拦截器如何选择过滤是否拦截方法，除了这里还有NonAspectAttribute 和全局的NonPredicates可以影响过滤
    bool CanAspect(MethodInfo method);
}
```

### 拦截器结类型

拦截器实际从设计上只有`IInterceptor`这一个统一的定义，不过由于csharp的单继承和`Attribute`的语言限制，所以有`AbstractInterceptorAttribute` 和 `AbstractInterceptor`两个类。

#### AbstractInterceptorAttribute （显示拦截器）

``` csharp 
public abstract class AbstractInterceptorAttribute : Attribute, IInterceptor
{
    public virtual int Order { get; set; }

    public virtual bool CanAspect(MethodInfo method) => true;

    // 默认提供在同步拦截器方法中转换异步方法为同步方式调用，存在一些性能损失，如果用户想要减少这方面的损耗，可以选择重载实现。
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

    // 默认只需要实现异步拦截器方法
    public abstract Task InvokeAsync(AspectContext context, AsyncAspectDelegate next);
}
```

一个拦截器实现举例：

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

##### `InterceptorAttribute`拦截器使用方式

- interface / class / method 可以设置 `Attribute`，如

``` csharp 
[AddTenInterceptor]
public interface IGenericTest<T, R> : IDisposable
{
    // or
    //[AddTenInterceptor]
    T GetT();
}
```

- 全局拦截器中也可以设置

``` csharp 
public void ConfigureServices(IServiceCollection services)
{
    services.ConfigureAop(i => i.GlobalInterceptors.Add(new AddTenInterceptorAttribute()));
}
```

#### AbstractInterceptor

和 `AbstractInterceptorAttribute` 几乎一模一样，不过不是`Attribute`，不能用于对应场景，只能在全局拦截器中使用。其实本身就是提供给用户用于不想`Attribute`场景简化Interceptor创建。

##### `Interceptor`拦截器使用方式

只能在全局拦截器中设置

``` csharp 
public void ConfigureServices(IServiceCollection services)
{
    services.ConfigureAop(i => i.GlobalInterceptors.Add(new AddSixInterceptor()));
}
```

### 全局拦截器 vs 显示拦截器

- 全局拦截器，是针对所有可以代理的方法都会做拦截，只需一次声明，全局有效

``` csharp 
public void ConfigureServices(IServiceCollection services)
{
    services.ConfigureAop(i => i.GlobalInterceptors.Add(new AddSixInterceptor()));
}
```

- 显示拦截器必须使用`AbstractInterceptorAttribute`在所有需要的地方都显示声明

``` csharp 
[AddTenInterceptor]
public interface IGenericTest<T, R> : IDisposable
{
    // or
    //[AddTenInterceptor]
    T GetT();
}
```

所以用户觉得怎么样方便就怎么用就好了

### 拦截器的过滤方式

Norns.Urd 提供如下三种过滤方式

- 全局过滤

``` csharp 
services.ConfigureAop(i => i.NonPredicates.AddNamespace("Norns")
    .AddNamespace("Norns.*")
    .AddNamespace("System")
    .AddNamespace("System.*")
    .AddNamespace("Microsoft.*")
    .AddNamespace("Microsoft.Owin.*")
    .AddMethod("Microsoft.*", "*"));
```

- 显示过滤

``` csharp 
[NonAspect]
public interface IGenericTest<T, R> : IDisposable
{
}
```

- 拦截器本身的过滤

``` csharp 
public class ParameterInjectInterceptor : AbstractInterceptor
{
    public override bool CanAspect(MethodInfo method)
    {
        return method.GetReflector().Parameters.Any(i => i.IsDefined<InjectAttribute>());
    }
}
```

### AOP限制

- 当 service type 为 class 时， 只有 virtual 且 子类能有访问的 方法才能代理拦截
- 有方法参数为 in readonly struct 的类型无法代理

## Interface和Abstract Class的默认实现

如果你向DI框架注册没有真正有具体实现的 `Interface`和`Abstract Class`, Norns.Urd 会实现默认的子类型。

为什么提供这样的功能呢？

这是为声明式编码思想提供一些底层实现支持，这样有更多的同学可以自定义自己的一些声明式库，简化代码，比如实现一个 声明式HttpClient 

### 默认实现限制

- 不支持属性注入
- Norns.Urd 生成的默认实现皆为返回类型的默认值

### demo

后面会完成一个简单的httpclient作为示例，这里先做个简单demo

1. 假如要加 10 就是我们类似http调用的逻辑，我们就可以讲全部的加10逻辑放在拦截器中

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

2. 定义声明式client

``` csharp 
[AddTen]
public interface IAddTest
{
    int AddTen();

    // 对于接口中的默认实现，并不会被Norns.Urd替代,这样可以提供某些场景用户可以自定义实现逻辑
    public int NoAdd() => 3;
}
```

3. 注册client

``` csharp 
services.AddTransient<IAddTest>();
services.ConfigureAop();
```

4. 使用

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

`InjectAttribute` 是对 Interface和Abstract Class的默认实现的功能补充，

特别是在做声明式client之类，提供自定义设置，比如interface 默认接口实现时，

用户可能需要从DI中获取实例，所以这里提供两种方式做一些补充。

### ParameterInject

方法参数可以设置`InjectAttribute`：
- 当参数为null时，就会从 DI 中尝试获取实例
- 当参数不为null时，不会覆盖传值，依然时传参值

示例：

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

按照业界编码习惯， field 不推荐没有赋值就是使用，所以该功能会导致代码检查出现需要修复的问题

``` csharp
public class ParameterInjectTest : IInjectTest
{
    [Inject]
    ParameterInjectInterceptorTest ft;
}
```

## FallbackAttribute

``` csharp
    public class DoFallbackTest
    {
        [Fallback(typeof(TestFallback))] // just need set Interceptor Type
        public virtual int Do(int i)
        {
            throw new FieldAccessException();
        }

        [Fallback(typeof(TestFallback))]
        public virtual Task<int> DoAsync(int i)
        {
            throw new FieldAccessException();
        }
    }

    public class TestFallback : AbstractInterceptor
    {
        public override void Invoke(AspectContext context, AspectDelegate next)
        {
            context.ReturnValue = (int)context.Parameters[0];
        }

        public override Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            var t = Task.FromResult((int)context.Parameters[0]);
            context.ReturnValue = t;
            return t;
        }
    }

```

## Polly

[Polly](https://github.com/App-vNext/Polly) is .NET resilience and transient-fault-handling library.

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
[Timeout(seconds: 1)]  // timeout 1 seconds, when timeout will throw TimeoutRejectedException
double Wait(double seconds);

[Timeout(timeSpan: "00:00:00.100")]  // timeout 100 milliseconds, only work on async method when no CancellationToken
async Task<double> WaitAsync(double seconds, CancellationToken cancellationToken = default);

[Timeout(timeSpan: "00:00:01")]  // timeout 1 seconds, but no work on async method when no CancellationToken
async Task<double> NoCancellationTokenWaitAsync(double seconds);
```

### RetryAttribute

``` csharp
[Retry(retryCount: 2, ExceptionType = typeof(AccessViolationException))]  // retry 2 times when if throw Exception
void Do()
```

### CircuitBreakerAttribute

``` csharp
[CircuitBreaker(exceptionsAllowedBeforeBreaking: 3, durationOfBreak: "00:00:01")]  
//or
[AdvancedCircuitBreaker(failureThreshold: 0.1, samplingDuration: "00:00:01", minimumThroughput: 3, durationOfBreak: "00:00:01")]
void Do()
```

### BulkheadAttribute

``` csharp
[Bulkhead(maxParallelization: 5, maxQueuingActions: 10)]
void Do()
```

## CacheAttribute

Norns.Urd 本身并未提供任何实际处理的缓存实现，

但基于`Microsoft.Extensions.Caching.Memory.IMemoryCache` 和 `Microsoft.Extensions.Caching.Distributed.IDistributedCache` 实现了`CacheAttribute`这一调用适配器

### 缓存策略

Norns.Urd适配了三种时间策略模式

* AbsoluteExpiration

绝对时间过期，意思是到了设置时间就过期

``` csharp
[Cache(..., AbsoluteExpiration = "1991-05-30 00:00:00")]
void Do()
```

* AbsoluteExpirationRelativeToNow

相对当前时间过了多次设置时间时才过期，也就是存活时间设置，意思为到了缓存设置起效时间 (1991-05-30 00:00:00) + 缓存有效时间 (05:00:00) = (1991-05-30 05:00:00) 时才过期

``` csharp
[Cache(..., AbsoluteExpirationRelativeToNow = "00:05:00")] // 存活 5 分钟
void Do()
```

### 启用内存缓存

``` csharp
IServiceCollection.ConfigureAop(i => i.EnableMemoryCache())
```

### 启用 DistributedCache

目前默认提供了`System.Text.Json`的序列化适配器

``` csharp
IServiceCollection.ConfigureAop(i => i.EnableDistributedCacheSystemTextJsonAdapter(/*可以指定自己的Name*/))
.AddDistributedMemoryCache() // 可以切换为任意的DistributedCache实现
```

* SlidingExpiration

滑动时间窗口过期，意思时缓存有效期内有任何访问都会让时间窗口有效期往后滑动，只有没有任何访问且过期才会缓存作废

``` csharp
[Cache(..., SlidingExpiration = "00:00:05")]
void Do()
```

### 使用缓存

#### 单一缓存

``` csharp
[Cache(cacheKey: "T", SlidingExpiration = "00:00:01")]  // 不指定缓存名，会使用 CacheOptions.DefaultCacheName = "memory"
public virtual Task<int> DoAsync(int count);
```

### 多级缓存

``` csharp
[Cache(cacheKey: nameof(Do), AbsoluteExpirationRelativeToNow = "00:00:01", Order = 1)]  // 先从内存缓存中获取，1秒后过期
[Cache(cacheKey: nameof(Do), cacheName："json", AbsoluteExpirationRelativeToNow = "00:00:02", Order = 2)] // 内存缓存失效后，会从 DistributedCache中获取
public virtual int Do(int count);
```

### 自定义缓存配置

很多时候，我们需要动态获取缓存配置，只需继承`ICacheOptionGenerator`，就可以自定义配置

举例如：

``` csharp
public class ContextKeyFromCount : ICacheOptionGenerator
{
    public CacheOptions Generate(AspectContext context)
    {
        return new CacheOptions()
        {
            CacheName = "json",
            CacheKey = context.Parameters[0],
            SlidingExpiration = TimeSpan.Parse("00:00:01")
        };
    }
}
```

使用：

``` csharp
[Cache(typeof(ContextKeyFromCount))]
public virtual Task<int> DoAsync(string key, int count)；
```

### 如何自定义新增 DistributedCache 序列化适配器

只需继承`ISerializationAdapter`就可以了

举例如：

``` csharp
public class SystemTextJsonAdapter : ISerializationAdapter
{
    public string Name { get; }

    public SystemTextJsonAdapter(string name)
    {
        Name = name;
    }

    public T Deserialize<T>(byte[] data)
    {
        return JsonSerializer.Deserialize<T>(data);
    }

    public byte[] Serialize<T>(T data)
    {
        return JsonSerializer.SerializeToUtf8Bytes<T>(data);
    }
}
```

注册：

``` csharp
public static IAspectConfiguration EnableDistributedCacheSystemTextJsonAdapter(this IAspectConfiguration configuration, string name = "json")
{
    return configuration.EnableDistributedCacheSerializationAdapter(i => new SystemTextJsonAdapter(name));
}
```

## HttpClient

这里的HttpClient是对 `System.Net.Http`下的 HttpClient封装，让大家只需简单在接口定义就可以实现http的调用，可以减少一些重复代码的书写。

### 如何启用 HttpClient 功能

1. 引入Norns.Urd.HttpClient
```
dotnet add package Norns.Urd.HttpClient
```

2. 代码中开启 HttpClient 功能，只需

``` csharp
new ServiceCollection()
    .ConfigureAop(i => i.EnableHttpClient())
```

3. 定义 要使用的 HttpClient 接口

举例如：

``` csharp
[BaseAddress("http://localhost.:5000")]
public interface ITestClient
{
    [Get("WeatherForecast/file")]
    [AcceptOctetStream]
    Task<Stream> DownloadAsync();

    [Post("WeatherForecast/file")]
    [OctetStreamContentType]
    Task UpoladAsync([Body]Stream f);
}
```

4. 注册到 ioc 中

``` csharp
new ServiceCollection()
    .AddSingleton<ITestClient>()  // 按照自己需要设置生命周期就好，并且不需要写具体实现，Norns.Urd.HttpClient会为您生成对应IL代码
    .ConfigureAop(i => i.EnableHttpClient())
```

5. 通过DI 使用就好， 比如

``` csharp
[ApiController]
[Route("[controller]")]
public class ClientController : ControllerBase
{
    private readonly ITestClient client;

    public ClientController(ITestClient client)
    {
        this.client = client;
    }

    [HttpGet("download")]
    public async Task<object> DownloadAsync()
    {
        using var r = new StreamReader(await client.DownloadAsync());
        return await r.ReadToEndAsync();
    }
}
```

### HttpClient支持的功能

#### Url 配置

##### BaseAddress

如果有些网站域名或者基础api地址都是很多接口都会使用的，就可以在接口上使用 `BaseAddressAttribute`

如：

``` csharp
[BaseAddress("http://localhost.:5000")]
public interface ITestClient
```
##### 各种 Http Method 支持设置Url

Http Method 支持如下：

- GetAttribute
- PostAttribute
- PutAttribute
- DeleteAttribute
- PatchAttribute
- OptionsAttribute
- HeadAttribute

（上述method 不够使用时，可以继承`HttpMethodAttribute` 自定义实现）

所有的这些Http Method都支持配置Url，有以下两种方式支持：

* 静态配置

``` csharp
[Post("http://localhost.:5000/money/getData/")]
public Data GetData()
```

* 动态配置

默认支持从 `IConfiguration` 通过key获取 url 配置

``` csharp
[Post("configKey", IsDynamicPath = true)]
public Data GetData()
```

如果这样简单的配置形式不支持您的需求，可以实现 `IHttpRequestDynamicPathFactory` 接口替换配置实现方式，实现好的类只需注册到IOC容器就可以了。
实现示例可以参考 `ConfigurationDynamicPathFactory`

##### 路由参数设置

如果某些url 路由参数需要动态设置，您可以通过`RouteAttribute`设置, 如

```csharp
[Post("getData/{id}")]
public Data GetData([Route]string id)
```

如果参数名字不匹配url里面的设置，可以通过`Alias =` 设置，如
```csharp
[Post("getData/{id}")]
public Data GetData([Route(Alias = "id")]string number)
```

##### Query string 设置

Query string参数可以在方法参数列表中设置

```csharp
[Post("getData")]
public Data GetData([Query]string id);
//or
[Post("getData")]
public Data GetData([Query(Alias = "id")]string number);
```

其Url结果都为 `getData?id=xxx`,

参数类型支持基本类型和 class，
当为class 时，会取class的属性作为参数，
所以当属性名不匹配定义时，可以在属性上用 `[Query(Alias = "xxx")]`指定

#### Request body 设置 

Request body 可以通过可以在方法参数列表中设置`BodyAttribute`指定参数，
需注意，只有第一个有`BodyAttribute`的参数会生效， 举例如

```csharp
public void SetData([Body]Data data);
```

将根据设置的 Request Content-Type 选择序列化器序列化body

#### Response body 设置

Response body 的类型指定，只需在方法的 return type 写上需要的类型就好，支持以下

- void        (忽略反序列化)
- Task        (忽略反序列化)
- ValueTask   (忽略反序列化)
- T
- Task<T>
- ValueTask<T>
- HttpResponseMessage
- Stream      (只能Content-Type为 application/octet-stream 时起效)

举例如：
```csharp
public Data GetData();
```

#### Content-Type 设置

无论 Request 还是 Response 的 Content-Type 都会影响 序列化和反序列化器的选择，

默认支持json/xml的序列化和反序列化，可以通过如下设置

- JsonContentTypeAttribute
- XmlContentTypeAttribute
- OctetStreamContentTypeAttribute

举例如：
```csharp
[OctetStreamContentType]
public Data GetData([Body]Stream s);
```

对应的Accept 设置为

- AcceptJsonAttribute
- AcceptXmlAttribute
- AcceptOctetStreamAttribute

举例如：
```csharp
[AcceptOctetStream]
public Stream GetData();
```

json 序列化器默认为 `System.Text.Json`

#### 更换json 序列化器为 NewtonsoftJson

1. 引入 `Norns.Urd.HttpClient.NewtonsoftJson`
2. 在 ioc 注册方法, 如

```csharp
new ServiceCollection().AddHttpClientNewtonsoftJosn()
```

#### 自定义序列化器

当现有序列化器不足以支持需求时，
只需实现 `IHttpContentSerializer` 并向 ioc 容器注册即可

#### 自定义 Header

除了上述已经提到的header之外，还可以通过添加其他header
同样有以下两种方式：

* 使用`HeaderAttribute`在接口或方法静态配置

```csharp
[Header("x-data", "money")]
public interface ITestClient {}
//or
[Header("x-data", "money")]
public Data GetData();
```

* 方法参数动态配置

```csharp
public Data GetData([SetRequestHeader("x-data")]string header);
```

#### 自定义HttpRequestMessageSettingsAttribute

当现有`HttpRequestMessageSettingsAttribute`不足以支持需求时，
只需继承 `HttpRequestMessageSettingsAttribute` 实现自己的功能，
在对应的接口/方法使用即可

#### 通过参数设置获取 Response Header

当有时我们需要获取 response 返回的 header 时，
我们可以 out 参数 + `OutResponseHeaderAttribute` 获取 Response Header的值
（需注意， 只有同步方法， out参数才能起作用）

举例如：

```csharp
public Data GetData([OutResponseHeader("x-data")] out string header);
```

#### HttpClient 一些参数设置方法

##### MaxResponseContentBufferSize

``` csharp
[MaxResponseContentBufferSize(20480)]
public interface ITestClient {}
//or
[MaxResponseContentBufferSize(20480)]
public Data GetData()
```

##### Timeout

``` csharp
[Timeout("00:03:00")]
public interface ITestClient {}
//or
[Timeout("00:03:00")]
public Data GetData()
```

##### ClientName

当需要结合 HttpClientFactory 获取特殊设置的 HttpClient 时，可以通过`ClientNameAttribute` 指定

如

``` csharp
[ClientName("MyClient")]
public interface ITestClient {}
//or
[ClientName("MyClient")]
public Data GetData()
```

就可以获取到这样指定的HttpClient

``` csharp
services.AddHttpClient("MyClient", i => i.MaxResponseContentBufferSize = 204800);
```

##### HttpCompletionOption

HttpClient 调用时的 CompletionOption 参数同样可以设置

HttpCompletionOption.ResponseHeadersRead 是默认配置

如

``` csharp
[HttpCompletionOption(HttpCompletionOption.ResponseContentRead)]
public interface ITestClient {}
//or
[HttpCompletionOption(HttpCompletionOption.ResponseContentRead)]
public Data GetData()
```

#### 全局 HttpRequestMessage 和 HttpResponseMessage 处理

如果需要全局对 HttpRequestMessage 和 HttpResponseMessage 做一些处理，比如：
- 链路追踪id 设置
- response 异常自定义处理

可以通过实现`IHttpClientHandler`并向ioc 容器注册使用
举例默认的 status code 检查 ，如：

``` csharp
public class EnsureSuccessStatusCodeHandler : IHttpClientHandler
{
    public int Order => 0;

    public Task SetRequestAsync(HttpRequestMessage message, AspectContext context, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public Task SetResponseAsync(HttpResponseMessage resp, AspectContext context, CancellationToken token)
    {
        resp.EnsureSuccessStatusCode();
        return Task.CompletedTask;
    }
}
```

当然，如果该 StatusCode 检查处理不需要的话，可以直接在ioc 容器清除掉， 如：
``` csharp
services.RemoveAll<IHttpClientHandler>();
// 然后添加自己的处理
services.AddSingleton<IHttpClientHandler, xxx>();
```

# Norns.Urd 中的一些设计

## Norns.Urd的实现前提

由于Norns.Urd的实现基于以下两点前提

1. 将 sync 和 async 方法同时兼容且如何将实现选择权完全交予用户

    - 其实这点还好，工作量变成两倍多一些就好，sync 和 async 完全拆分成两套实现。
    - 提供给用户的Interceptor接口要提供 sync 和 async 混合在一套实现代码的方案，毕竟不能强迫用户实现两套代码，很多场景用户不需要为sync 和 async 的差异而实现两套代码

2. 不包含任何内置DI，但要整体都为支持DI而作

    - 其实如果内置DI容器可以让支持 generic 场景变得非常简单，毕竟从DI容器中实例化对象时必须有明确的类型，但是呢，现在已经有了那么多实现的库了，我就不想为了一些场景而实现很多功能（我真的懒，否则这个库也不会写那么久了）
    - 但是DI容器确实解耦非常棒，我自己都常常因此受益而减少了很多代码修改量，所以做一个aop库必须要考虑基于DI容器做支持，这样的话，di 支持的 open generic / 自定义实例化方法都要做支持，并且aop里面还得提供用户调用DI的方法，否则还不好用了 （这样算下来，我真的偷懒了吗？我是不是在给自己挖坑呀？） 

## 如何设计解决的？ 

目前方案不一定完美，暂时算解决了问题而已 （有更好方案请一定要告诉我，我迫切需要学习）

### 提供什么样的拦截器编写模式给用户？

以前接触一些其他aop实现框架，很多都需要将拦截代码分为 方法前 / 方法后 / 有异常等等，个人觉得这样的形式还是一定程度上影响拦截器实现的代码思路，总觉得不够顺滑

但是像 [ASP.NET Core Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/?view=aspnetcore-5.0)就感觉非常不错，如下图和代码：

![https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/index/_static/request-delegate-pipeline.png?view=aspnetcore-5.0](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/index/_static/request-delegate-pipeline.png?view=aspnetcore-5.0)

``` csharp
app.Run(async context =>
{
    await context.Response.WriteAsync("Hello, World!");
});
```

拦截器也应该可以像这样做，所以拦截器的代码应该可以像这样：

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

### sync 和 async 方法如何拆分？又如何能合并在一起呢？用户有怎么自己选择实现sync 还是 async 或者两个都都实现呢？

``` csharp

public delegate Task AsyncAspectDelegate(AspectContext context);

public delegate void AspectDelegate(AspectContext context);

// 拆分： 
// 由AspectDelegate 和 AsyncAspectDelegate 建立两套完全区分 sync 和 async 的Middleware调用链，具体使用哪个由具体被拦截的方法本身决定

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

// 合并：
// 默认实现转换方法内容，这样各种拦截器都可以混在一个Middleware调用链中

    public abstract Task InvokeAsync(AspectContext context, AsyncAspectDelegate next);

// 用户自主性选择：
// 同时提供sync 和 async 拦截器方法可以重载，用户就可以自己选择了
// 所以用户在 async 中可以调用专门的未异步优化代码了，也不用说在 sync 中必须 awit 会影响性能了，
// 你认为影响性能，你在乎就自己都重载，不在乎那就自己选
}
```

### 没有内置DI，如何兼容其他DI框架呢？

DI框架都有注册类型，我们可以通过 emit 生成代理类，替换原本的注册，就可以做到兼容。

当然每种DI框架都需要定制化的实现一些代码才能支持（唉，又是工作量呀）

### `AddTransient<IMTest>(x => new NMTest())`, 类似这样的实例化方法怎么支持呢？

由于这种DI框架的用法，无法通过Func函数拿到实际会使用的类型，只能根据IMTest定义通过emit 生成 桥接代理类型，其伪码类似如下：

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

### `.AddTransient(typeof(IGenericTest<,>), typeof(GenericTest<,>))` 类似这样的 Open generic 怎么支持呢？

其实对于泛型，我们通过 emit 生成泛型类型一点问题都没有，唯一的难点是不好生成 `Get<T>()` 这样的方法调用， 因为IL需要反射找到的具体方法，比如`Get<int>()` `Get<bool>()` 等等，不能是不明确的 `Get<T>()`。

要解决这个问题就只能将实际的调用延迟到运行时调用再生成具体的调用，伪码大致如下：

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
