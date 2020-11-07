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
#pragma warning disable IDE0052 // Remove unread private members
        private readonly IAspectConfiguration configuration;
#pragma warning restore IDE0052 // Remove unread private members

        public InterceptorCreator(IAspectConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public Type CreateProxyType(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
