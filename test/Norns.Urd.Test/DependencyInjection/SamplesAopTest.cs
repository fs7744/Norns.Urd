using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Moq;
using System;
using System.IO;
using Xunit;

namespace Norns.Urd.Test.DependencyInjection
{
    public abstract class TConsoleFormatter
    {
        protected TConsoleFormatter(string name)
        {
            Name = name;
        }

        public string Name { get; }
       public abstract void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider scopeProvider, TextWriter textWriter);
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
            var provider = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddSingleton<TConsoleFormatter>().AddSingleton("D"));
            var p = provider.GetRequiredService<TConsoleFormatter>();
            Assert.NotNull(p);
        }
    }
}