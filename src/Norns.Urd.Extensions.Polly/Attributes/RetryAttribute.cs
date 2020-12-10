using Norns.Urd.Reflection;
using Polly;
using System;
using System.Reflection;

namespace Norns.Urd.Extensions.Polly.Attributes
{
    public class RetryAttribute : AbstractPolicyAttribute
    {
        private static MethodInfo HandleException = typeof(Policy).GetMethod(nameof(Policy.Handle), Type.EmptyTypes);
        private readonly int retryCount;
        public Type ExceptionType { get; set; } = typeof(Exception);
        public RetryAttribute(int retryCount)
        {
            this.retryCount = retryCount;
        }

        public override ISyncPolicy Build(MethodReflector method)
        {
            return CreatePolicy().Retry(retryCount);
        }

        private PolicyBuilder CreatePolicy()
        {
            return ((PolicyBuilder)HandleException.MakeGenericMethod(ExceptionType).Invoke(null, null));
        }

        public override IAsyncPolicy BuildAsync(MethodReflector method)
        {
            return CreatePolicy().RetryAsync(retryCount);
        }
    }
}