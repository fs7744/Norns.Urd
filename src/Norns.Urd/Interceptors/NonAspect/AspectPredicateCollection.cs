using Norns.Urd.Interceptors;
using System.Collections.Generic;

namespace Norns.Urd
{
    public class AspectPredicateCollection
    {
        public List<AspectTypePredicate> TypePredicates { get; } = new List<AspectTypePredicate>();
        public List<AspectMethodPredicate> MethodPredicates { get; } = new List<AspectMethodPredicate>();

        public AspectTypePredicate BuildNonAspectTypePredicate() => t =>
        {
            foreach (var item in TypePredicates)
            {
                if (item(t))
                    return true;
            }
            return false;
        };

        public AspectMethodPredicate BuildNonAspectMethodPredicate() => m =>
        {
            foreach (var item in MethodPredicates)
            {
                if (item(m))
                    return true;
            }
            return false;
        };
    }
}