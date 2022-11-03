using Norns.Urd.Interceptors;
using Norns.Urd.Reflection;
using System;
using System.Collections.Generic;
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
        public const string ServiceProviderProperty = "ServiceProviderGenerated";
        public static readonly Type[] DefaultConstructorParameters = new Type[] { typeof(IServiceProvider) };
        public static readonly ConstructorInfo ObjectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.Single();
        public static readonly HashSet<string> IgnoreMethods = new HashSet<string> { "Finalize", "ToString", "Equals", "GetHashCode", "Void Dispose()_Base" };
        public static readonly MethodInfo GetReturnValue = typeof(AspectContext).GetProperty(nameof(AspectContext.ReturnValue)).GetMethod;
        public static readonly MethodInfo SetReturnValue = typeof(AspectContext).GetProperty(nameof(AspectContext.ReturnValue)).SetMethod;
        public static readonly ConstructorInfo AspectContextCtor = typeof(AspectContext).GetConstructors().First();
        public static readonly MethodInfo GetTypeFromHandle = MethodExtensions.GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));
        public static readonly MethodInfo GetMethodFromHandle = MethodExtensions.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));
        public static readonly MethodInfo Invoke = typeof(AspectDelegate).GetMethod(nameof(AspectDelegate.Invoke));
        public static readonly MethodInfo InvokeAsync = typeof(AsyncAspectDelegate).GetMethod(nameof(AsyncAspectDelegate.Invoke));
        public static readonly MethodInfo GetInterceptor = typeof(IInterceptorCreator).GetMethod(nameof(IInterceptorCreator.GetInterceptor));
        public static readonly MethodInfo GetInterceptorAsync = typeof(IInterceptorCreator).GetMethod(nameof(IInterceptorCreator.GetInterceptorAsync));
        public static readonly MethodInfo GetOpenGenericInterceptor = typeof(IInterceptorCreator).GetMethod(nameof(IInterceptorCreator.GetOpenGenericInterceptor));
        public static readonly MethodInfo GetOpenGenericInterceptorAsync = typeof(IInterceptorCreator).GetMethod(nameof(IInterceptorCreator.GetOpenGenericInterceptorAsync));
        public static readonly MethodInfo GetParameters = typeof(AspectContext).GetProperty(nameof(AspectContext.Parameters)).GetMethod;
        public static readonly MethodInfo GetService = typeof(AspectContext).GetProperty(nameof(AspectContext.Service)).GetMethod;
        public static readonly MethodInfo Await = typeof(InterceptorCreator).GetMethod(nameof(InterceptorCreator.Await));
        public static readonly MethodInfo AwaitValueTask = typeof(InterceptorCreator).GetMethod(nameof(InterceptorCreator.AwaitValueTask));
        public static readonly MethodInfo AwaitValueTaskReturnValue = typeof(InterceptorCreator).GetMethod(nameof(InterceptorCreator.AwaitValueTaskReturnValue));
        public static readonly BindingFlags MethodBindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        public static readonly MethodInfo GetServiceFromDI = typeof(IServiceProvider).GetMethod(nameof(IServiceProvider.GetService));
        public static readonly MethodInfo PropertyInject = typeof(PropertyInjector).GetMethod(nameof(PropertyInjector.Inject));
        public static readonly MethodInfo ConverTotReturnTask = typeof(AopInitializer).GetMethod(nameof(AopInitializer.ConverTotReturnTask));
        public static readonly MethodInfo ConverTotReturnValueTask = typeof(AopInitializer).GetMethod(nameof(AopInitializer.ConverTotReturnValueTask));
        public static readonly MethodInfo GetReturnTask = typeof(AspectContext).GetProperty(nameof(AspectContext.ReturnTask)).GetMethod;


    }
}