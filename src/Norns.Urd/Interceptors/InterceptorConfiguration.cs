using System;

namespace Norns.Urd.Interceptors
{
    public interface IInterceptorConfiguration
    {
        bool IsIgnoreType(Type serviceType);
    }

    public class InterceptorConfiguration : IInterceptorConfiguration
    {
        public bool IsIgnoreType(Type serviceType)
        {
            return false;
        }
    }
}