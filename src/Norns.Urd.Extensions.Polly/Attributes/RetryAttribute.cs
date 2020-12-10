using Norns.Urd.Reflection;
using Polly;
using System;

namespace Norns.Urd.Extensions.Polly.Attributes
{
    public class RetryAttribute : AbstractPolicyAttribute
    {
        private readonly int retryCount;

        public RetryAttribute(int retryCount)
        {
            this.retryCount = retryCount;
        }

        public override ISyncPolicy Build(MethodReflector method)
        {
            return Policy.Handle<Exception>().Retry(retryCount);
        }

        public override IAsyncPolicy BuildAsync(MethodReflector method)
        {
            return Policy.Handle<Exception>().RetryAsync(retryCount);
        }
    }
}