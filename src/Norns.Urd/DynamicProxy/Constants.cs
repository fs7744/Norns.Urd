using System;
using System.Linq;
using System.Reflection;

namespace Norns.Urd.DynamicProxy
{
    public static class Constants
    {
        public const string GeneratedNamespace = "Norns.Urd.DynamicProxy.Generated";
        public const string Init = "Init";
        public const string Instance = "instanceGenerated";
        public const string ServiceProvider = "serviceProviderGenerated";
        public static readonly Type[] DefaultConstructorParameters = new Type[] { typeof(IServiceProvider) };
        public static readonly ConstructorInfo ObjectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.Single();
    }
}