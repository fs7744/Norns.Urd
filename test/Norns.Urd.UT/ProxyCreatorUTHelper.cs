using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Proxy;
using System.Reflection;

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
                .AddSingleton<IInterceptorFactory, InterceptorFactory>()
                .AddSingleton<IAspectConfiguration>(new AspectConfiguration())
                .BuildServiceProvider()
                .GetRequiredService<IProxyCreator>();
        }
    }

    public class TestInterceptorFactory : IInterceptorFactory
    {
        public void CreateInterceptor(MethodInfo method)
        {
            throw new System.NotImplementedException();
        }
    }
}