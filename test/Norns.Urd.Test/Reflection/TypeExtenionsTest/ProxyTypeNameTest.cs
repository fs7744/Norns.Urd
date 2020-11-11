using Norns.Urd.DynamicProxy;
using Norns.Urd.Reflection;
using System;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;

namespace Norns.Urd.Test.Reflection.TypeExtenionsTest
{
    public class ProxyTypeNameTest
    {
        private readonly ModuleBuilder moduleBuilder;

        public interface INestedProxyTypeNameTest
        {
        }

        public class NestedProxyTypeNameTest
        {
        }

        public class NestedProxyTypeNameTest<T, R>
        {
        }

        public ProxyTypeNameTest()
        {
            var asmBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Norns.Urd.Test.Reflection.TypeExtenionsTest.Generated"), AssemblyBuilderAccess.RunAndCollect);
            moduleBuilder = asmBuilder.DefineDynamicModule("core");
        }

        [Theory]
        [InlineData(typeof(ProxyTypeNameTest), ProxyTypes.Facade, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest_Proxy_Facade")]
        [InlineData(typeof(ProxyTypeNameTest), ProxyTypes.Inherit, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest_Proxy_Inherit")]
        [InlineData(typeof(NestedProxyTypeNameTest), ProxyTypes.Facade, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest.NestedProxyTypeNameTest_Proxy_Facade")]
        [InlineData(typeof(NestedProxyTypeNameTest), ProxyTypes.Inherit, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest.NestedProxyTypeNameTest_Proxy_Inherit")]
        [InlineData(typeof(INestedProxyTypeNameTest), ProxyTypes.Facade, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest.INestedProxyTypeNameTest_Proxy_Facade")]
        [InlineData(typeof(INestedProxyTypeNameTest), ProxyTypes.Inherit, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest.INestedProxyTypeNameTest_Proxy_Inherit")]
        [InlineData(typeof(NestedProxyTypeNameTest<,>), ProxyTypes.Facade, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest.NestedProxyTypeNameTest<T\\,R>_Proxy_Facade")]
        [InlineData(typeof(NestedProxyTypeNameTest<,>), ProxyTypes.Inherit, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest.NestedProxyTypeNameTest<T\\,R>_Proxy_Inherit")]
        [InlineData(typeof(NestedProxyTypeNameTest<int, long>), ProxyTypes.Facade, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest.NestedProxyTypeNameTest<Int32\\,Int64>_Proxy_Facade")]
        [InlineData(typeof(NestedProxyTypeNameTest<int, long>), ProxyTypes.Inherit, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest.NestedProxyTypeNameTest<Int32\\,Int64>_Proxy_Inherit")]
        public void WhenNonGenericInstanceClass(Type type, ProxyTypes proxyType, string name)
        {
            var pt = moduleBuilder.DefineProxyType(type.GetTypeInfo(), proxyType).CreateType();
            Assert.NotNull(pt);
            Assert.Equal(name, pt.FullName);
        }
    }
}