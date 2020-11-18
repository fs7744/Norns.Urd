using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using Xunit;

namespace Norns.Urd.Test.DependencyInjection
{
    public abstract class LoggerT<T> : ILogger, ILogger<T>
    {
        public  IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
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
    }
}