using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Proxy;

namespace Norns.Urd.UT
{
    public static class ProxyCreatorUTHelper
    {
        public static IProxyCreator InitPorxyCreator()
        {
            return new ServiceCollection()
                .AddSingleton<IProxyGenerator, FacadeProxyGenerator>()
                .AddSingleton<IProxyGenerator, InheritProxyGenerator>()
                .AddSingleton<IProxyCreator, ProxyCreator>()
                .BuildServiceProvider()
                .GetRequiredService<IProxyCreator>();
        }
    }

    public class TestInterceptorFactory : IInterceptorFactory
    { 
        
    }
}