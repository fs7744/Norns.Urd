using System;
using System.Collections.Generic;
using System.Text;

namespace Norns.Urd
{
    public interface IInterceptorCreator
    {
        Type CreateProxyType(Type type);
    }

    public class InterceptorCreator : IInterceptorCreator
    {
        //private readonly IAspectConfiguration configuration;

        //public InterceptorCreator(IAspectConfiguration configuration)
        //{
        //    this.configuration = configuration;
        //}

        public Type CreateProxyType(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
