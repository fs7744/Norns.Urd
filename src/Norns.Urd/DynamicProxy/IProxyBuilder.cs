using Norns.Urd.Interceptors;
using System;
using System.Reflection.Emit;

namespace Norns.Urd.DynamicProxy
{
    public interface IProxyBuilder
    {
        Type Create(Type serviceType, IInterceptorCreator interceptorCreator, ModuleBuilder moduleBuilder);
    }
}