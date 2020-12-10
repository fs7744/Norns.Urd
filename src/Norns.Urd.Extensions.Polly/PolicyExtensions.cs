using Polly;
using System;
using System.Reflection;

namespace Norns.Urd.Extensions.Polly
{
    public static class PolicyExtensions
    {
        internal static readonly MethodInfo HandleException = typeof(Policy).GetMethod(nameof(Policy.Handle), Type.EmptyTypes);

        public static PolicyBuilder CreatePolicyBuilder(Type exceptionType)
        {
            return ((PolicyBuilder)PolicyExtensions.HandleException.MakeGenericMethod(exceptionType).Invoke(null, null));
        }
        public static IAspectConfiguration EnablePolly(this IAspectConfiguration configuration)
        {
            configuration.GlobalInterceptors.Add(new PolicyInterceptor());
            return configuration;
        }
    }
}