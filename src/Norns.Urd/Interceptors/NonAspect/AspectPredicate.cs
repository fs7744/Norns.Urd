using System;
using System.Reflection;

namespace Norns.Urd.Interceptors
{
    public delegate bool AspectTypePredicate(Type type);

    public delegate bool AspectMethodPredicate(MethodInfo method);
}