using Norns.Urd.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Norns.Urd.Utils
{
    public class ConstantInfo
    {
        public static readonly MethodInfo GetTypeFromHandle = InternalExtensions.GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));
        
        public static readonly MethodInfo GetMethodFromHandle = InternalExtensions.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));
        
        public readonly ConstructorInfo ArgumentNullExceptionCtor = typeof(ArgumentNullException).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });
        
        public readonly ConstructorInfo ObjectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.Single();
        
        public readonly Type AspectContextType = typeof(AspectContext);
        
        public readonly ConstructorInfo AspectContextCtor = typeof(AspectContext).GetConstructors().First();
        
        public readonly MethodInfo Invoke = typeof(AspectDelegate).GetMethod(nameof(AspectDelegate.Invoke));

        public readonly MethodInfo InvokeAsync = typeof(AspectDelegateAsync).GetMethod(nameof(AspectDelegateAsync.Invoke));

        public readonly MethodInfo GetReturnValue = typeof(AspectContext).GetProperty(nameof(AspectContext.ReturnValue)).GetMethod;

        public readonly HashSet<string> IgnoreMethods = new HashSet<string> { "Finalize", "ToString", "Equals", "GetHashCode" };

        public readonly MethodInfo GetInterceptor = typeof(IInterceptorFactory).GetMethod(nameof(IInterceptorFactory.GetInterceptor));

        public readonly MethodInfo GetInterceptorAsync = typeof(IInterceptorFactory).GetMethod(nameof(IInterceptorFactory.GetInterceptorAsync));
    }
}
