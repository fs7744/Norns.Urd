using Norns.Urd.Extensions.Polly;
using Polly.Bulkhead;
using Xunit;

namespace Test.Norns.Urd.Polly
{
    public class BulkheadTest
    {
        [Fact]
        public void WhenCircuitBreaker()
        {
            var bulkhead = new BulkheadAttribute(3, 2);
            Assert.IsType<BulkheadPolicy>(bulkhead.Build(CircuitBreakerTest.MR));
            Assert.IsType<AsyncBulkheadPolicy>(bulkhead.BuildAsync(CircuitBreakerTest.MR));
        }
    }
}