using System;

namespace Norns.Urd.Interceptors
{
    public static class NonAspectPredicates
    {
        public static NonAspectTypePredicate ForNameSpace(string nameSpace) => type => type.Namespace.Matches(nameSpace);

        public static NonAspectTypePredicate ForService(string service) => type =>
        {
            return type.Name.Matches(service)
                || (type.FullName ?? $"{type.Namespace}.{type.Name}").Matches(service);
        };

        public static NonAspectMethodPredicate ForMethod(string method) => methodInfo => methodInfo.Name.Matches(method);

        public static NonAspectMethodPredicate ForMethod(string service, string method)
        {
            var forService = ForService(service);
            return methodInfo => forService(methodInfo.DeclaringType) && methodInfo.Name.Matches(method);
        }

        public static NonAspectTypePredicate Implement(Type baseOrInterfaceType) => type => baseOrInterfaceType.IsAssignableFrom(type);
    }
}