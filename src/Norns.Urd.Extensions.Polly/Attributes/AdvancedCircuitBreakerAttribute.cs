using Norns.Urd.Reflection;
using Polly;
using System;

namespace Norns.Urd.Extensions.Polly
{
    public class AdvancedCircuitBreakerAttribute : AbstractPolicyAttribute
    {
        private readonly double failureThreshold;
        private readonly TimeSpan samplingDuration;
        private readonly int minimumThroughput;
        private readonly TimeSpan durationOfBreak;

        public Type ExceptionType { get; set; } = typeof(Exception);

        public AdvancedCircuitBreakerAttribute(double failureThreshold, string samplingDuration, int minimumThroughput, string durationOfBreak)
        {
            this.failureThreshold = failureThreshold;
            this.samplingDuration = TimeSpan.Parse(samplingDuration);
            this.minimumThroughput = minimumThroughput;
            this.durationOfBreak = TimeSpan.Parse(durationOfBreak);
        }

        public override ISyncPolicy Build(MethodReflector method)
        {
            return PolicyExtensions.CreatePolicyBuilder(ExceptionType)
                .AdvancedCircuitBreaker(failureThreshold, samplingDuration, minimumThroughput, durationOfBreak);
        }

        public override IAsyncPolicy BuildAsync(MethodReflector method)
        {
            return PolicyExtensions.CreatePolicyBuilder(ExceptionType)
                .AdvancedCircuitBreakerAsync(failureThreshold, samplingDuration, minimumThroughput, durationOfBreak);
        }
    }
}