using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Norns.Urd
{
    public class InterceptorFilterCollection 
    {
        private readonly List<Func<MethodInfo, bool>> methodFilters = new List<Func<MethodInfo, bool>>() { m => true};
        private readonly List<Func<Type, bool>> typeFilters = new List<Func<Type, bool>>() { m => true };

        public void NamespaceNotStartWith(string @namespace)
        {
            methodFilters.Add(i => !i.DeclaringType.Namespace.StartsWith(@namespace));
            typeFilters.Add(i => !i.Namespace.StartsWith(@namespace));
        }

        internal bool CanAspect(Type type)
        {
            return typeFilters.All(i => i(type));
        }

        internal bool CanAspect(MethodInfo method)
        {
            return methodFilters.All(i => i(method));
        }
    }
}