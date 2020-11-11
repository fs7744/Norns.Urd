using Norns.Urd.DynamicProxy;
using Norns.Urd.Reflection;
using System;
using System.Linq;
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

        public class NestedProxyTypeNameTest<T>
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
        [InlineData(typeof(NestedProxyTypeNameTest<>), ProxyTypes.Facade, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest.NestedProxyTypeNameTest<T>_Proxy_Facade")]
        [InlineData(typeof(NestedProxyTypeNameTest<>), ProxyTypes.Inherit, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest.NestedProxyTypeNameTest<T>_Proxy_Inherit")]
        [InlineData(typeof(NestedProxyTypeNameTest<int>), ProxyTypes.Facade, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest.NestedProxyTypeNameTest<Int32>_Proxy_Facade")]
        [InlineData(typeof(NestedProxyTypeNameTest<int>), ProxyTypes.Inherit, "Norns.Urd.DynamicProxy.Generated.ProxyTypeNameTest.NestedProxyTypeNameTest<Int32>_Proxy_Inherit")]
        public void WhenNonGenericInstanceClass(Type type, ProxyTypes proxyType, string name)
        {
            Assert.Equal(name, type.GetTypeInfo().GetProxyTypeName(proxyType));
            var pType = type.IsInterface
                ? moduleBuilder.DefineType(name, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, typeof(object), new Type[] { type })
                : moduleBuilder.DefineType(name, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, type, Type.EmptyTypes);
            DefineGenericParameter(type, pType);
            var pt = pType.CreateType();
            Assert.NotNull(pt);
        }

        public static void DefineGenericParameter(Type targetType, TypeBuilder typeBuilder)
        {
            if (!targetType.GetTypeInfo().IsGenericTypeDefinition)
            {
                return;
            }
            var genericArguments = targetType.GetTypeInfo().GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
            var genericArgumentsBuilders = typeBuilder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
            for (var index = 0; index < genericArguments.Length; index++)
            {
                genericArgumentsBuilders[index].SetGenericParameterAttributes(ToClassGenericParameterAttributes(genericArguments[index].GenericParameterAttributes));
                foreach (var constraint in genericArguments[index].GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                {
                    if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                    if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                }
            }
        }

        public static GenericParameterAttributes ToClassGenericParameterAttributes(GenericParameterAttributes attributes)
        {
            if (attributes == GenericParameterAttributes.None)
            {
                return GenericParameterAttributes.None;
            }
            if (attributes.HasFlag(GenericParameterAttributes.SpecialConstraintMask))
            {
                return GenericParameterAttributes.SpecialConstraintMask;
            }
            if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            {
                return GenericParameterAttributes.NotNullableValueTypeConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                return GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                return GenericParameterAttributes.ReferenceTypeConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                return GenericParameterAttributes.DefaultConstructorConstraint;
            }
            return GenericParameterAttributes.None;
        }
    }
}