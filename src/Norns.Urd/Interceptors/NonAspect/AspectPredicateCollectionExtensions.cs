using Norns.Urd.Interceptors;
using Norns.Urd.Reflection;
using System.Reflection;

namespace Norns.Urd
{
    public static class AspectPredicateCollectionExtensions
    {
        public static AspectPredicateCollection AddType(this AspectPredicateCollection collection, AspectTypePredicate predicate)
        {
            collection.TypePredicates.Add(predicate);
            return collection;
        }

        public static AspectPredicateCollection AddMethod(this AspectPredicateCollection collection, AspectMethodPredicate predicate)
        {
            collection.MethodPredicates.Add(predicate);
            return collection;
        }

        public static AspectPredicateCollection AddMethod(this AspectPredicateCollection collection, string service, string method)
        {
            collection.AddMethod(AspectPredicates.ForMethod(service, method));
            return collection;
        }

        public static AspectPredicateCollection AddMethod(this AspectPredicateCollection collection, string method)
        {
            collection.AddMethod(AspectPredicates.ForMethod(method));
            return collection;
        }

        public static AspectPredicateCollection AddNamespace(this AspectPredicateCollection collection, string nameSpace)
        {
            collection.AddType(AspectPredicates.ForNameSpace(nameSpace));
            return collection;
        }

        public static AspectPredicateCollection AddService(this AspectPredicateCollection collection, string service)
        {
            collection.AddType(AspectPredicates.ForService(service));
            return collection;
        }

        public static AspectPredicateCollection Clean(this AspectPredicateCollection collection)
        {
            collection.TypePredicates.Clear();
            collection.MethodPredicates.Clear();
            collection.AddDefault();
            return collection;
        }

        internal static AspectPredicateCollection AddDefaultCore(this AspectPredicateCollection collection)
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

        internal static AspectPredicateCollection AddDefault(this AspectPredicateCollection collection)
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