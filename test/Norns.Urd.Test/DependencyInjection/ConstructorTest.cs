using Microsoft.Extensions.DependencyInjection;
using Norns.Urd;
using Norns.Urd.Reflection;
using System;
using System.Reflection;
using Xunit;

namespace Test.Norns.Urd.DependencyInjection
{
    public class TestDataAttribute : Attribute
    {
        public TestDataAttribute(int v)
        {
            V = v;
        }

        public int V { get; }
    }

    public interface INoConstructorsInterface
    {
        [Inject]
        OneConstructorNoArgs A { get; set; }

        [Inject]
        OneConstructorNoArgs B { get; }
    }

    public class OneConstructorNoArgs
    {
    }

    public class OneConstructorOneArgs
    {
        public readonly ConstructorTest A;

        public OneConstructorOneArgs(ConstructorTest a)
        {
            A = a;
        }
    }

    [TestData(2)]
    public class TwoConstructorOneArgs
    {
        public readonly ConstructorTest A;

        [Inject]
        private OneConstructorNoArgs AA { get; set; }

        [Inject]
        private OneConstructorNoArgs B { get; }

        [Inject]
        public virtual ConstructorTest AAa { get; set; }

        [Inject]
        public virtual ConstructorTest Ba { get; }

        [TestData(99)]
        public TwoConstructorOneArgs([TestData(4)] ConstructorTest a)
        {
            A = a;
        }

        public TwoConstructorOneArgs()
        {
        }
    }

    public abstract class AbstractTwoConstructorOneArgs
    {
        public readonly ConstructorTest A;

        protected AbstractTwoConstructorOneArgs(ConstructorTest a)
        {
            A = a;
        }

        protected AbstractTwoConstructorOneArgs()
        {
        }
    }

    public class OneConstructorOneStringArgs
    {
        public virtual string A { get; set; }

        public OneConstructorOneStringArgs(string a)
        {
            A = a;
        }
    }

    [NonAspect]
    public class ConstructorTest
    {
        [Fact]
        public void WhenNoConstructorsInterface()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<INoConstructorsInterface>()
                    .AddTransient<OneConstructorNoArgs>())
                .GetRequiredService<INoConstructorsInterface>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.Null(p.B);
            Assert.Null(p.A);
        }

        [Fact]
        public void WhenOneConstructorNoArgs()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<OneConstructorNoArgs>())
                .GetRequiredService<OneConstructorNoArgs>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
        }

        [Fact]
        public void WhenOneConstructorOneArgs()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<OneConstructorOneArgs>().AddTransient<ConstructorTest>())
                .GetRequiredService<OneConstructorOneArgs>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p.A);
        }

        [Fact]
        public void WhenTwoConstructorOneArgsAndCustomAttribute()
        {
            var v = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<TwoConstructorOneArgs>().AddTransient<ConstructorTest>())
              .GetRequiredService<TwoConstructorOneArgs>();
            var pt = v.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(pt.CreateServiceProviderGetter()(v));
            Assert.NotNull(v.A);
            Assert.Equal(2, pt.GetCustomAttribute<TestDataAttribute>().V);
            Assert.Equal(99, pt.GetConstructors()[0].GetCustomAttribute<TestDataAttribute>().V);
            Assert.Equal(4, pt.GetConstructors()[0].GetParameters()[0].GetCustomAttribute<TestDataAttribute>().V);
            Assert.NotNull(v.AAa);
            Assert.Null(v.Ba);
        }

        [Fact]
        public void WhenAbstractTwoConstructorOneArgs()
        {
            var v = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<AbstractTwoConstructorOneArgs>().AddTransient<ConstructorTest>())
              .GetRequiredService<AbstractTwoConstructorOneArgs>();
            var pt = v.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(pt.CreateServiceProviderGetter()(v));
            Assert.NotNull(v);
            Assert.NotNull(v.A);
        }

        [Fact]
        public void WhenOneConstructorOneStringArgs()
        {
            var v = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddSingleton(i => new OneConstructorOneStringArgs("a")))
                .GetRequiredService<OneConstructorOneStringArgs>();
            var pt = v.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(pt.CreateInstanceGetter()(v));
            Assert.NotNull(pt.CreateServiceProviderGetter()(v));
            Assert.NotNull(v);
            Assert.NotNull(v.A);
            Assert.Equal("a", v.A);
        }
    }
}