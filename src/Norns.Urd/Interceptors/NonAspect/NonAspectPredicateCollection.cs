using System.Collections.Generic;
using System.Linq;

namespace Norns.Urd.Interceptors
{
    public class NonAspectPredicateCollection
    {
        public List<NonAspectTypePredicate> TypePredicates { get; } = new List<NonAspectTypePredicate>();
        public List<NonAspectMethodPredicate> MethodPredicates { get; } = new List<NonAspectMethodPredicate>();

        public NonAspectTypePredicate BuildNonAspectTypePredicate()
        {
            var predicates = TypePredicates;
            return predicates.Skip(1).Aggregate(predicates.First(), (i, j) => x => i(x) || j(x));
        }

        public NonAspectMethodPredicate BuildNonAspectMethodPredicate()
        {
            var predicates = MethodPredicates;
            return predicates.Skip(1).Aggregate(predicates.First(), (i, j) => x => i(x) || j(x));
        }
    }
}