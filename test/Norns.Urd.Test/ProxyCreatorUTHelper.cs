using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Proxy;
using Norns.Urd.Utils;

namespace Norns.Urd.UT
{
    public static class ProxyCreatorUTHelper
    {
        public static (IProxyCreator, IInterceptorFactory, IAspectConfiguration) InitPorxyCreator()
        {
            var p = new ServiceCollection()
                .AddSingleton<IProxyGenerator, FacadeProxyGenerator>()
                .AddSingleton<IProxyGenerator, InheritProxyGenerator>()
                .AddSingleton<IProxyCreator, ProxyCreator>()
                .AddSingleton<ConstantInfo>()
                .AddSingleton<IInterceptorFactory, InterceptorFactory>()
                .AddSingleton<IAspectConfiguration>(new AspectConfiguration())
                .BuildServiceProvider();
            return (p.GetRequiredService<IProxyCreator>(), p.GetRequiredService<IInterceptorFactory>(), p.GetRequiredService<IAspectConfiguration>());
        }
    }
}