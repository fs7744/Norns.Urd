using Norns.Urd.Extensions;
using System;
using System.Linq;
using System.Reflection;

namespace Norns.Urd.Utils
{
    internal static class MethodInfoConstant
    {
        internal static readonly MethodInfo GetTypeFromHandle = InternalExtensions.GetMethod<Func<RuntimeTypeHandle, Type>>(handle => Type.GetTypeFromHandle(handle));

        internal static readonly MethodInfo GetMethodFromHandle = InternalExtensions.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2));

        internal static readonly ConstructorInfo ArgumentNullExceptionCtor = typeof(ArgumentNullException).GetTypeInfo().GetConstructor(new Type[] { typeof(string) });

        internal static readonly ConstructorInfo ObjectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.Single();
    }
}
