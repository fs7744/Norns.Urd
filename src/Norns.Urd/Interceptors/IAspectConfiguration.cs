using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Norns.Urd
{
    public interface IAspectConfiguration
    {
        List<IInterceptor> GlobalInterceptors { get; }

        NonAspectPredicateCollection NonPredicates { get; }

        List<Action<IServiceCollection>> ConfigServices { get; }
    }

    public class AspectConfiguration : IAspectConfiguration
    {
        public List<IInterceptor> GlobalInterceptors { get; } = new List<IInterceptor>();

        public NonAspectPredicateCollection NonPredicates { get; } = new NonAspectPredicateCollection().AddDefault();

        public List<Action<IServiceCollection>> ConfigServices { get; } = new List<Action<IServiceCollection>>();
    }
}