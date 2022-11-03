using System;
using System.Linq;
using System.Collections.Generic;

namespace Norns.Urd.Interceptors
{
    public class InterceptorCollection : List<IInterceptor>
    {
        public InterceptorCollection TryAdd<T>(Func<T> creator) where T : IInterceptor
        {
            if (this.All(i => !(i is T)))
            {
                Add(creator());
            }
            return this;
        }
    }
}