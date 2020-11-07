using Norns.Urd.Extensions;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

namespace Norns.Urd.Test.Extensions
{
    public class TypeExtensionsTests
    {
        [Fact]
        public void IsNullableType()
        {
            Assert.False(typeof(int).GetTypeInfo().IsNullableType());
            Assert.True(typeof(int?).GetTypeInfo().IsNullableType());
            Assert.False(typeof(object).GetTypeInfo().IsNullableType());
        }

        [Fact]
        public void IsTask()
        {
            var typeInfo = null as TypeInfo;
            var exception = Record.Exception(() =>
            {
                typeInfo.IsTask();
            });
            Assert.IsType<ArgumentNullException>(exception);

            var task = new Task(() => { });
            var valueTask = new ValueTask();
            Assert.True(task.GetType().GetTypeInfo().IsTask());
            Assert.False(valueTask.GetType().GetTypeInfo().IsTask());
        }

        [Fact]
        public void IsTaskWithResult()
        {
            var typeInfo = null as TypeInfo;
            var exception = Record.Exception(() =>
            {
                typeInfo.IsTaskWithResult();
            });
            Assert.IsType<ArgumentNullException>(exception);

            var task1 = new Task(() => { });
            var task2 = new Task<int>(() =>
            {
                return 0;
            });
            Assert.False(task1.GetType().GetTypeInfo().IsTaskWithResult());
            Assert.True(task2.GetType().GetTypeInfo().IsTaskWithResult());
        }

        [Fact]
        public void IsValueTask()
        {
            var typeInfo = null as TypeInfo;
            var exception = Record.Exception(() =>
            {
                typeInfo.IsValueTask();
            });
            Assert.IsType<ArgumentNullException>(exception);
            var task = new Task(() => { });
            var valueTask = new ValueTask();
            Assert.False(task.GetType().GetTypeInfo().IsValueTask());
            Assert.True(valueTask.GetType().GetTypeInfo().IsValueTask());
        }

        [Fact]
        public void IsValueTaskWithResult()
        {
            var typeInfo = null as TypeInfo;
            var exception = Record.Exception(() =>
            {
                typeInfo.IsValueTaskWithResult();
            });
            Assert.IsType<ArgumentNullException>(exception);
            var valueTask1 = new ValueTask();
            var valueTask2 = new ValueTask<int>(0);
            Assert.False(valueTask1.GetType().GetTypeInfo().IsValueTaskWithResult());
            Assert.True(valueTask2.GetType().GetTypeInfo().IsValueTaskWithResult());
        }

        [Theory]
        [InlineData(typeof(void))]
        [InlineData(typeof(DateTime))]
        [InlineData(typeof(UInt64))]
        [InlineData(typeof(FakePublicVisible))]
        public void GetDefaultValue(Type type)
        {
            var typeInfo = null as TypeInfo;
            var exception = Record.Exception(() =>
            {
                typeInfo.IsValueTaskWithResult();
            });
            Assert.IsType<ArgumentNullException>(exception);

            var value = type.GetTypeInfo().GetDefaultValue();
            if (type == typeof(void) || type == typeof(String))
            {
                Assert.Null(value);
            }
            else if (type == typeof(DateTime))
            {
                var expected = default(DateTime);
                Assert.Equal(expected, value);
            }
            else if (type == typeof(UInt64))
            {
                Assert.Equal(0, value);
            }
        }

        [Fact]
        public void TypeVisible()
        {
            Assert.True(typeof(FakePublicVisible).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakeNonPublicVisible).GetTypeInfo().IsVisible());
        }

        [Fact]
        public void GenericTypeVisible()
        {
            Assert.True(typeof(FakeGenericVisible<FakePublicVisible>).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakeGenericVisible<FakeNonPublicVisible>).GetTypeInfo().IsVisible());
            Assert.True(typeof(FakeGenericVisible<FakePublicVisible.FakePublicNestedVisible>).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakeGenericVisible<FakePublicVisible.FakeNonPublicNestedVisible>).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakeGenericVisible<FakeNonPublicVisible.FakePublicNestedVisible>).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakeGenericVisible<FakePublicVisible.FakeNonPublicNestedVisible>).GetTypeInfo().IsVisible());
        }

        [Fact]
        public void NestedTypeVisible()
        {
            Assert.True(typeof(FakePublicVisible.FakePublicNestedVisible).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakePublicVisible.FakeNonPublicNestedVisible).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakeNonPublicVisible.FakePublicNestedVisible).GetTypeInfo().IsVisible());
            Assert.False(typeof(FakePublicVisible.FakeNonPublicNestedVisible).GetTypeInfo().IsVisible());
        }
    }

    public class FakePublicVisible
    {
        public class FakePublicNestedVisible { }
        internal class FakeNonPublicNestedVisible { }
    }

    internal class FakeNonPublicVisible
    {
        public class FakePublicNestedVisible { }
        internal class FakeNonPublicNestedVisible { }
    }

    public class FakeGenericVisible<T>
    {
        public T Data;
    }
}
