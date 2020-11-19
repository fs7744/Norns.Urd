using Norns.Urd.Interceptors;
using System.Collections.Generic;

namespace Norns.Urd
{
    public interface IAspectConfiguration
    {
        List<IInterceptor> GlobalInterceptors { get; }

        NonAspectPredicateCollection NonPredicates { get; }
    }

    public class AspectConfiguration : IAspectConfiguration
    {
        public List<IInterceptor> GlobalInterceptors { get; } = new List<IInterceptor>();

        public NonAspectPredicateCollection NonPredicates { get; } = new NonAspectPredicateCollection().AddDefault();
    }
}