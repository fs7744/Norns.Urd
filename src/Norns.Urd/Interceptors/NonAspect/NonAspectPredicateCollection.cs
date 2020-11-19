using Norns.Urd.Interceptors;
using System.Collections.Generic;

namespace Norns.Urd
{
    public class NonAspectPredicateCollection
    {
        public List<NonAspectTypePredicate> TypePredicates { get; } = new List<NonAspectTypePredicate>();
        public List<NonAspectMethodPredicate> MethodPredicates { get; } = new List<NonAspectMethodPredicate>();

        public NonAspectTypePredicate BuildNonAspectTypePredicate() => t =>
        {
            foreach (var item in TypePredicates)
            {
                if (item(t))
                    return true;
            }
            return false;
        };

        public NonAspectMethodPredicate BuildNonAspectMethodPredicate() => m =>
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