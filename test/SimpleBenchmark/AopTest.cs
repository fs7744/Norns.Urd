using AspectCore.Extensions.DependencyInjection;
using BenchmarkDotNet.Attributes;
using Castle.Core;
using Castle.DynamicProxy;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Microsoft.Extensions.DependencyInjection;
using Norns.Urd;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleBenchmark
{
    public interface ISum
    {
        string Add(string a, string b);

        Task<string> AddAsync(string a, string b);
    }

    [AspectCoreAddA]
    public class SumAspectCore : ISum
    {
        public string Add(string a, string b) => a + b;

        public Task<string> AddAsync(string a, string b) => Task.FromResult(a + b);
    }

    [NornsUrdAddA]
    public class SumNornsUrd : ISum
    {
        public string Add(string a, string b) => a + b;

        public Task<string> AddAsync(string a, string b) => Task.FromResult(a + b);
    }

    public class Sum : ISum
    {
        public string Add(string a, string b) => a + b;

        public Task<string> AddAsync(string a, string b) => Task.FromResult(a + b);
    }

    public class SumWithAddA : ISum
    {
        private readonly ISum sum = new Sum();

        public string Add(string a, string b) => sum.Add(a, b) + "A";

        public async Task<string> AddAsync(string a, string b) => (await sum.AddAsync(a, b)) + "A";
    }

    public class CastleAddAInterceptor : Castle.DynamicProxy.IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
            var v = invocation.ReturnValue;
            if (v is string s)
            {
                invocation.ReturnValue = s + "A";
            }
            else if (v is Task<string> t)
            {
                invocation.ReturnValue = Task.FromResult(t.Result + "A");
            }
        }
    }

    public class NornsUrdAddAAttribute : AbstractInterceptorAttribute
    {
        public override void Invoke(AspectContext context, AspectDelegate next)
        {
            next(context);
            context.ReturnValue = context.ReturnValue.ToString() + "A";
        }

        public override async Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            await next(context);
            var v = context.ReturnValue;
            if (v is Task<string> t)
            {
                context.ReturnValue = Task.FromResult(t.Result + "A");
            }
        }
    }

    public class AspectCoreAddAAttribute : AspectCore.DynamicProxy.AbstractInterceptorAttribute
    {
        public override async Task Invoke(AspectCore.DynamicProxy.AspectContext context, AspectCore.DynamicProxy.AspectDelegate next)
        {
            await next(context);
            var v = context.ReturnValue;
            if (v is string s)
            {
                context.ReturnValue = s + "A";
            }
            else if (v is Task<string> t)
            {
                context.ReturnValue = Task.FromResult(t.Result + "A");
            }
        }
    }

    [MemoryDiagnoser]
    public class AopTest
    {
        private readonly IServiceProvider nornsUrdProvider;
        private readonly IServiceProvider aspectCoreProvider;
        private readonly WindsorContainer castleProvider;
        private readonly IServiceProvider provider;

        public AopTest()
        {
            aspectCoreProvider = new ServiceCollection()
                .AddTransient<ISum, SumAspectCore>()
                .BuildServiceContextProvider();
            nornsUrdProvider = new ServiceCollection()
                .AddTransient<ISum, SumNornsUrd>()
                .ConfigureAop()
                .BuildServiceProvider();
            castleProvider = new WindsorContainer();
            castleProvider.Register(Component.For<CastleAddAInterceptor>().Named("test"));
            castleProvider.Register(Component.For<ISum>().ImplementedBy<Sum>().Interceptors(InterceptorReference.ForKey("test")).Anywhere);
            provider = new ServiceCollection().AddTransient<ISum, SumWithAddA>()
                .BuildServiceProvider();
        }

        public void Check()
        {
            Assert.Equal("abA", provider.GetRequiredService<ISum>().Add("a", "b"));
            Assert.Equal("abA", nornsUrdProvider.GetRequiredService<ISum>().Add("a", "b"));
            Assert.Equal("abA", castleProvider.Resolve<ISum>().Add("a", "b"));
            Assert.Equal("abA", aspectCoreProvider.GetRequiredService<ISum>().Add("a", "b"));

            Assert.Equal("abA", provider.GetRequiredService<ISum>().AddAsync("a", "b").Result);
            Assert.Equal("abA", nornsUrdProvider.GetRequiredService<ISum>().AddAsync("a", "b").Result);
            Assert.Equal("abA", castleProvider.Resolve<ISum>().AddAsync("a", "b").Result);
            Assert.Equal("abA", aspectCoreProvider.GetRequiredService<ISum>().AddAsync("a", "b").Result);
        }

        [Benchmark]
        public void TransientInstanceCallSyncMethodWhenNoAop()
        {
            provider.GetRequiredService<ISum>().Add("a", "b");
        }

        [Benchmark]
        public void TransientInstanceCallSyncMethodWhenNornsUrd()
        {
            nornsUrdProvider.GetRequiredService<ISum>().Add("a", "b");
        }

        [Benchmark]
        public void TransientInstanceCallSyncMethodWhenCastle()
        {
            castleProvider.Resolve<ISum>().Add("a", "b");
        }

        [Benchmark]
        public void TransientInstanceCallSyncMethodWhenAspectCore()
        {
            aspectCoreProvider.GetRequiredService<ISum>().Add("a", "b");
        }

        [Benchmark]
        public async Task TransientInstanceCallAsyncMethodWhenNoAop()
        {
            await provider.GetRequiredService<ISum>().AddAsync("a", "b");
        }

        [Benchmark]
        public async Task TransientInstanceCallAsyncMethodWhenNornsUrd()
        {
            await nornsUrdProvider.GetRequiredService<ISum>().AddAsync("a", "b");
        }

        [Benchmark]
        public async Task TransientInstanceCallAsyncMethodWhenCastle()
        {
            await castleProvider.Resolve<ISum>().AddAsync("a", "b");
        }

        [Benchmark]
        public async Task TransientInstanceCallAsyncMethodWhenAspectCore()
        {
            await aspectCoreProvider.GetRequiredService<ISum>().AddAsync("a", "b");
        }
    }
}