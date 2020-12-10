using Norns.Urd.Reflection;
using Polly;
using System;

namespace Norns.Urd.Extensions.Polly
{
    public class CircuitBreakerAttribute : AbstractPolicyAttribute
    {
        private readonly int exceptionsAllowedBeforeBreaking;
        private readonly TimeSpan durationOfBreak;

        public Type ExceptionType { get; set; } = typeof(Exception);

        public CircuitBreakerAttribute(int exceptionsAllowedBeforeBreaking, string durationOfBreak)
        {
            this.exceptionsAllowedBeforeBreaking = exceptionsAllowedBeforeBreaking;
            this.durationOfBreak = TimeSpan.Parse(durationOfBreak);
        }

        public override ISyncPolicy Build(MethodReflector method)
        {
            return PolicyExtensions.CreatePolicyBuilder(ExceptionType)
                .CircuitBreaker(exceptionsAllowedBeforeBreaking, durationOfBreak);
        }

        public override IAsyncPolicy BuildAsync(MethodReflector method)
        {
            return PolicyExtensions.CreatePolicyBuilder(ExceptionType)
                .CircuitBreakerAsync(exceptionsAllowedBeforeBreaking, durationOfBreak);
        }
    }

}