using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;
using Polly.Caching;
using Polly.Caching.Memory;
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
            configuration.NonPredicates.AddNamespace("Polly")
                .AddNamespace("Polly.*");
            configuration.GlobalInterceptors.Add(new PolicyInterceptor());
            configuration.ConfigServices.Add(services =>
            {
                services.AddMemoryCache();
                services.TryAddSingleton<ISyncCacheProvider>(i => new MemoryCacheProvider(i.GetRequiredService<IMemoryCache>()));
                services.TryAddSingleton(i => i.GetRequiredService<ISyncCacheProvider>() as IAsyncCacheProvider);
            });
            return configuration;
        }
    }
}