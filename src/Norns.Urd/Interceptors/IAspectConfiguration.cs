using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Interceptors;
using System;
using System.Collections.Generic;

namespace Norns.Urd
{
    public interface IAspectConfiguration
    {
        InterceptorCollection GlobalInterceptors { get; }

        AspectPredicateCollection NonPredicates { get; }

        AspectPredicateCollection FacdeProxyAllowPredicates { get; }

        List<Action<IServiceCollection>> ConfigServices { get; }
    }

    public class AspectConfiguration : IAspectConfiguration
    {
        public InterceptorCollection GlobalInterceptors { get; } = new InterceptorCollection();

        public AspectPredicateCollection NonPredicates { get; } = new AspectPredicateCollection().AddDefault();

        public List<Action<IServiceCollection>> ConfigServices { get; } = new List<Action<IServiceCollection>>();

        public AspectPredicateCollection FacdeProxyAllowPredicates { get; } = new AspectPredicateCollection();
    }
}