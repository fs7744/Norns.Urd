using Norns.Urd.Reflection;
using Polly;
using Polly.Timeout;
using System;

namespace Norns.Urd.Extensions.Polly
{
    public class TimeoutAttribute : AbstractPolicyAttribute
    {
        private readonly TimeSpan timeSpan;

        public TimeoutAttribute(int seconds)
        {
            timeSpan = TimeSpan.FromSeconds(seconds);
        }

        public TimeoutAttribute(string timeSpan)
        {
            this.timeSpan = TimeSpan.Parse(timeSpan);
        }

        public override ISyncPolicy Build(MethodReflector method)
        {
            return Policy.Timeout(timeSpan, TimeoutStrategy.Pessimistic);
        }

        public override IAsyncPolicy BuildAsync(MethodReflector method)
        {
            return Policy.TimeoutAsync(timeSpan, TimeoutStrategy.Optimistic);
        }
    }
}