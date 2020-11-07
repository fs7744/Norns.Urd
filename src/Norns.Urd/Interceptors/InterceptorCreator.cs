using System;

namespace Norns.Urd
{
    public interface IInterceptorCreator
    {
        Type CreateProxyType(Type type);
    }

    public class InterceptorCreator : IInterceptorCreator
    {
        public Type CreateProxyType(Type type)
        {
            throw new NotImplementedException();
        }
    }
}