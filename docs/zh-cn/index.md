# [See English](https://fs7744.github.io/Norns.Urd/index.html)

# 欢迎了解 Norns.Urd

Norns.Urd 是一个基于emit实现动态代理的轻量级AOP框架

完成这个框架的目的主要出自于个人以下意愿：

- 静态AOP和动态AOP都实现一次
- 如果不实现DI，怎么将AOP框架实现与其他现有DI框架集成
- 一个AOP 如何将 sync 和 async 方法同时兼容且如何将实现选择权完全交予用户

希望该库能对大家有些小小的作用

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
