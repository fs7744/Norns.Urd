using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Reflection;
using System;
using Xunit;

namespace Norns.Urd.Test.DependencyInjection
{
    public interface IGenericTest<T, R>
    {
        //T F { get; }
    }

    public interface ISealedGenericTest<T, R>
    {
        //T F { get; }
    }

    public sealed class SealedGenericTest<T, R> : ISealedGenericTest<T, R>
    {
        //public T F => default;
    }

    public class GenericTest<T, R> : IGenericTest<T, R>
    {
        //public T F => default;
    }

    public static class AopTestExtensions
    {
        public static IServiceProvider ConfigServiceCollectionWithAop(Func<IServiceCollection, IServiceCollection> config)
        {
            return config(new ServiceCollection())
            .ConfigureAop()
            .BuildServiceProvider();
        }
    }

    public class AopExtensionsTest
    {
        [Fact]
        public void WhenGenericTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IGenericTest<int, long>, GenericTest<int, long>>())
                 .GetRequiredService<IGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenOpenGenericTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient(typeof(IGenericTest<,>), typeof(GenericTest<,>)))
                 .GetRequiredService<IGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenSealedOpenGenericTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient(typeof(ISealedGenericTest<,>), typeof(SealedGenericTest<,>)))
                 .GetRequiredService<ISealedGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.False(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenSealedOpenGenericImplementationFactoryTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<ISealedGenericTest<int, long>>(i => new SealedGenericTest<int, long>()))
                .GetRequiredService<ISealedGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(pt.CreateInstanceGetter()(pt));
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenSealedOpenGenericImplementationTypeTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<ISealedGenericTest<int, long>, SealedGenericTest<int, long>>())
                .GetRequiredService<ISealedGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(pt.CreateInstanceGetter()(pt));
            Assert.NotNull(p);
        }
    }
}