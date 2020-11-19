using Norns.Urd.Interceptors;
using Norns.Urd.Reflection;
using System.Reflection;

namespace Norns.Urd
{
    public static class NonAspectPredicateCollectionExtensions
    {
        public static NonAspectPredicateCollection AddType(this NonAspectPredicateCollection collection, NonAspectTypePredicate predicate)
        {
            collection.TypePredicates.Add(predicate);
            return collection;
        }

        public static NonAspectPredicateCollection AddMethod(this NonAspectPredicateCollection collection, NonAspectMethodPredicate predicate)
        {
            collection.MethodPredicates.Add(predicate);
            return collection;
        }

        public static NonAspectPredicateCollection AddMethod(this NonAspectPredicateCollection collection, string service, string method)
        {
            collection.AddMethod(NonAspectPredicates.ForMethod(service, method));
            return collection;
        }

        public static NonAspectPredicateCollection AddMethod(this NonAspectPredicateCollection collection, string method)
        {
            collection.AddMethod(NonAspectPredicates.ForMethod(method));
            return collection;
        }

        public static NonAspectPredicateCollection AddNamespace(this NonAspectPredicateCollection collection, string nameSpace)
        {
            collection.AddType(NonAspectPredicates.ForNameSpace(nameSpace));
            return collection;
        }

        public static NonAspectPredicateCollection AddService(this NonAspectPredicateCollection collection, string service)
        {
            collection.AddType(NonAspectPredicates.ForService(service));
            return collection;
        }

        public static NonAspectPredicateCollection Clean(this NonAspectPredicateCollection collection)
        {
            collection.TypePredicates.Clear();
            collection.MethodPredicates.Clear();
            collection.AddDefaultCore();
            return collection;
        }

        internal static NonAspectPredicateCollection AddDefaultCore(this NonAspectPredicateCollection collection)
        {
            return collection
                .AddMethod("Equals")
                .AddMethod("GetHashCode")
                .AddMethod("ToString")
                .AddMethod("GetType")
                .AddMethod(m => m.DeclaringType == typeof(object))
                .AddMethod(i => i.GetReflector().IsDefined<NonAspectAttribute>())
                .AddType(i => !i.GetTypeInfo().IsVisible() || i.GetReflector().IsDefined<NonAspectAttribute>());
        }

        internal static NonAspectPredicateCollection AddDefault(this NonAspectPredicateCollection collection)
        {
            return collection
                .AddNamespace("Norns")
                .AddNamespace("Norns.*")
                .AddNamespace("System")
                .AddNamespace("System.*")
                .AddNamespace("Microsoft.*")
                .AddNamespace("Microsoft.Owin.*")
                .AddMethod("Microsoft.*", "*")
                .AddDefaultCore();
        }
    }
}