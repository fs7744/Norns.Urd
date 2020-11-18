using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Moq;
using Norns.Urd.Reflection;
using System;
using System.IO;
using Xunit;

namespace Norns.Urd.Test.DependencyInjection
{
    public class TConsoleFormatter
    {
        public virtual void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter) { }
    }

    public class A : ConsoleFormatter
    {
        public A(string name) : base(name)
        {
        }

        public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter)
        {
            throw new NotImplementedException();
        }
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

        [Fact]
        public void ConsoleFormatterAopTest()
        {
            var provider = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddSingleton<TConsoleFormatter>(), true);
            var p = provider.GetRequiredService<TConsoleFormatter>();
            Assert.False(p.GetType().IsProxyType());
            Assert.NotNull(p);
        }
    }
}