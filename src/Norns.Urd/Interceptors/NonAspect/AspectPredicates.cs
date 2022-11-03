using System;

namespace Norns.Urd.Interceptors
{
    public static class AspectPredicates
    {
        public static AspectTypePredicate ForNameSpace(string nameSpace) => type => type.Namespace.Matches(nameSpace);

        public static AspectTypePredicate ForService(string service) => type =>
        {
            return type.Name.Matches(service)
                || (type.FullName ?? $"{type.Namespace}.{type.Name}").Matches(service);
        };

        public static AspectMethodPredicate ForMethod(string method) => methodInfo => methodInfo.Name.Matches(method);

        public static AspectMethodPredicate ForMethod(string service, string method)
        {
            var forService = ForService(service);
            return methodInfo => forService(methodInfo.DeclaringType) && methodInfo.Name.Matches(method);
        }

        public static AspectTypePredicate Implement(Type baseOrInterfaceType) => type => baseOrInterfaceType.IsAssignableFrom(type);
    }
}