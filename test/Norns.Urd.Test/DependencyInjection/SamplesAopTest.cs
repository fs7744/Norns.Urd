using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Norns.Urd.Reflection;
using System.IO;
using Xunit;

namespace Test.Norns.Urd.DependencyInjection
{
    public class TConsoleFormatter
    {
        public virtual void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
        }
    }

    public class TestController : ControllerBase
    {
    }

    public class SamplesAopTest
    {
        [Fact]
        public void LoggerAopTest()
        {
            var provider = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient(typeof(ILogger<>), typeof(Logger<>))
                .AddSingleton(new Mock<ILoggerFactory>().Object));

            var p = provider.GetRequiredService<ILogger<SamplesAopTest>>();
            Assert.NotNull(p);
        }

        //todo : in parameter
        [Fact]
        public void ConsoleFormatterAopTest()
        {
            var provider = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddSingleton<TConsoleFormatter>(), true);
            var p = provider.GetRequiredService<TConsoleFormatter>();
            Assert.False(p.GetType().IsProxyType());
            Assert.NotNull(p);
        }

        [Fact]
        public void ControllerBaseAopTest()
        {
            var provider = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddSingleton<TestController>(), false);
            var p = provider.GetRequiredService<TestController>();
            Assert.True(p.GetType().IsProxyType());
            Assert.NotNull(p);
        }
    }
}