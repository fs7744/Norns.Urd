using Microsoft.Extensions.DependencyInjection;
using System;

namespace Norns.Urd.Test.DependencyInjection
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
            }, isIgnoreError)
            .BuildServiceProvider();
        }
    }
}