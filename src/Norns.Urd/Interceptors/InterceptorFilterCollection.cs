using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Norns.Urd
{
    public class InterceptorFilterCollection : List<Func<MethodInfo, bool>>
    {
        public void NamespaceNotStartWith(string @namespace)
        {
            Add(i => !i.DeclaringType.Namespace.StartsWith(@namespace));
        }

        internal bool CanAspect(MethodInfo method)
        {
            return this.All(i => i(method));
        }
    }
}