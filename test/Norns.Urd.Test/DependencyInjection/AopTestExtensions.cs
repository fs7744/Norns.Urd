using Microsoft.Extensions.DependencyInjection;
using Norns.Urd;
using System;

namespace Test.Norns.Urd.DependencyInjection
{
    public static class AopTestExtensions
    {
        public static IServiceCollection ConfigCollectionWithAop(Func<IServiceCollection, IServiceCollection> config, bool isIgnoreError = false)
        {
            return config(new ServiceCollection())
            .ConfigureAop(i =>
            {
                i.NonPredicates.Clean();
                i.GlobalInterceptors.Add(new AddTenInterceptor());
                i.FacdeProxyAllowPredicates.AddNamespace("*");
            }, isIgnoreError);
        }

        public static IServiceProvider ConfigServiceCollectionWithAop(Func<IServiceCollection, IServiceCollection> config, bool isIgnoreError = false)
        {
            return ConfigCollectionWithAop(config, isIgnoreError)
            .BuildServiceProvider();
        }
    }
}