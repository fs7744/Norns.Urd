using System;
using Xunit;

namespace Norns.Urd.Test
{
    public class AopInitializerTest
    {
        private int instanceGenerated;

        public interface INestedProxyTypeNameTest
        {
        }

        public class NestedProxyTypeNameTest : IDisposable
        {
            private Type instanceGenerated;

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        public class NestedProxyTypeNameTest<T, R>
        {
            private T instanceGenerated;
        }

        [Theory]
        [InlineData(typeof(AopInitializerTest), 4)]
        [InlineData(typeof(NestedProxyTypeNameTest), typeof(int))]
        [InlineData(typeof(NestedProxyTypeNameTest<int, long>), 4)]
        public void WhenHasInstanceField(Type type, object instance)
        {
            var set = type.CreateInstanceSetter();
            Assert.NotNull(set);
            var o = Activator.CreateInstance(type);
            set(o, instance);
            var instanceData = type.CreateInstanceGetter()(o);
            Assert.NotNull(instanceData);
            Assert.IsType(instance.GetType(), instanceData);
            Assert.Equal(instance, instanceData);
        }

        [Theory]
        [InlineData(typeof(INestedProxyTypeNameTest))]
        [InlineData(typeof(Action<int, long>))]
        public void WhenNoInstanceField(Type type)
        {
            var set = type.CreateInstanceSetter();
            Assert.Null(set);
        }
    }
}