using Norns.Urd.Reflection;
using Polly;
using System;

namespace Norns.Urd.Extensions.Polly
{
    public class RetryAttribute : AbstractPolicyAttribute
    {
        private readonly int retryCount;
        public Type ExceptionType { get; set; } = typeof(Exception);

        public RetryAttribute(int retryCount)
        {
            this.retryCount = retryCount;
        }

        public override ISyncPolicy Build(MethodReflector method)
        {
            return PolicyExtensions.CreatePolicyBuilder(ExceptionType).Retry(retryCount);
        }

        public override IAsyncPolicy BuildAsync(MethodReflector method)
        {
            return PolicyExtensions.CreatePolicyBuilder(ExceptionType).RetryAsync(retryCount);
        }
    }
}