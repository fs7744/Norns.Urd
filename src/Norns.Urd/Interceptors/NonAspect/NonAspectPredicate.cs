using System;
using System.Reflection;

namespace Norns.Urd.Interceptors
{
    public delegate bool NonAspectTypePredicate(Type type);

    public delegate bool NonAspectMethodPredicate(MethodInfo method);
}