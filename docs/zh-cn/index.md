#### [See English](https://fs7744.github.io/Norns.Urd/index.html)
# 目录
- [欢迎了解 Norns.Urd](#欢迎了解-nornsurd)
- [快速入门指南](#快速入门指南)
- [功能说明](#功能说明)
- [Norns.Urd 中的一些设计](#nornsurd-中的一些设计)
- [Nuget Packages](#nuget-packages)

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

# Nuget Packages

| Package Name |  NuGet | Downloads  |
|--------------|  ------- |  ----  |
| Norns.Urd | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd)](https://www.nuget.org/packages/Norns.Urd/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd) |
| Norns.Urd.Extensions.DependencyInjection | [![Nuget](https://img.shields.io/nuget/v/Norns.Urd.Extensions.DependencyInjection)](https://www.nuget.org/packages/Norns.Urd.Extensions.DependencyInjection/) | ![Nuget](https://img.shields.io/nuget/dt/Norns.Urd.Extensions.DependencyInjection) |