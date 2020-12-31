using Microsoft.Extensions.DependencyInjection;
using Norns.Urd;
using System;

namespace Test.Norns.Urd.DependencyInjection
{
    public static class AopTestExtensions
    {
        public static IServiceProvider ConfigServiceCollectionWithAop(Func<IServiceCollection, IServiceCollection> config, bool isIgnoreError = false)
        {
            return config(new ServiceCollection())
            .ConfigureAop(i =>
            {
                i.NonPredicates.Clean();
                i.GlobalInterceptors.Add(new AddTenInterceptor());
                i.FacdeProxyAllowPredicates.AddNamespace("*");
            }, isIgnoreError)
            .BuildServiceProvider();
        }
    }
}