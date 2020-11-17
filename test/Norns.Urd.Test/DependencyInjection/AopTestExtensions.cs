using Microsoft.Extensions.DependencyInjection;
using System;

namespace Norns.Urd.Test.DependencyInjection
{
    public static class AopTestExtensions
    {
        public static IServiceProvider ConfigServiceCollectionWithAop(Func<IServiceCollection, IServiceCollection> config)
        {
            return config(new ServiceCollection())
            .ConfigureAop(i => i.GlobalInterceptors.Add(new AddTenInterceptor()))
            .BuildServiceProvider();
        }
    }
}