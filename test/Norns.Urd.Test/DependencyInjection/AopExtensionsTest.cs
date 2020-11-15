using Microsoft.Extensions.DependencyInjection;
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
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenOpenGenericTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient(typeof(IGenericTest<,>), typeof(GenericTest<,>)))
                 .GetRequiredService<IGenericTest<int, long>>();
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenSealedOpenGenericImplementationFactoryTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<ISealedGenericTest<int, long>>(i => new SealedGenericTest<int, long>()))
                .GetRequiredService<ISealedGenericTest<int, long>>();
            Assert.NotNull(p);
        }
    }
}