using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Interceptors;
using System;
using System.Collections.Generic;

namespace Norns.Urd
{
    public interface IAspectConfiguration
    {
        InterceptorCollection GlobalInterceptors { get; }

        NonAspectPredicateCollection NonPredicates { get; }

        List<Action<IServiceCollection>> ConfigServices { get; }
    }

    public class AspectConfiguration : IAspectConfiguration
    {
        public InterceptorCollection GlobalInterceptors { get; } = new InterceptorCollection();

        public NonAspectPredicateCollection NonPredicates { get; } = new NonAspectPredicateCollection().AddDefault();

        public List<Action<IServiceCollection>> ConfigServices { get; } = new List<Action<IServiceCollection>>();
    }
}