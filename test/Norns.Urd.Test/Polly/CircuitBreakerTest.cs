using Norns.Urd.DynamicProxy;
using Norns.Urd.Extensions.Polly;
using Norns.Urd.Reflection;
using Polly.CircuitBreaker;
using Xunit;

namespace Test.Norns.Urd.Polly
{
    public class CircuitBreakerTest
    {
        private readonly MethodReflector MR = Constants.GetTypeFromHandle.GetReflector();

        [Fact]
        public void WhenCircuitBreaker()
        {
            var circuitBreaker = new CircuitBreakerAttribute(3, "00:00:01");
            Assert.IsType<CircuitBreakerPolicy>(circuitBreaker.Build(MR));
            Assert.IsType<AsyncCircuitBreakerPolicy>(circuitBreaker.BuildAsync(MR));
        }

        [Fact]
        public void WhenAdvancedCircuitBreaker()
        {
            var circuitBreaker = new AdvancedCircuitBreakerAttribute(0.1, "00:00:01", 3, "00:00:01");
            Assert.IsType<CircuitBreakerPolicy>(circuitBreaker.Build(MR));
            Assert.IsType<AsyncCircuitBreakerPolicy>(circuitBreaker.BuildAsync(MR));
        }
    }
}