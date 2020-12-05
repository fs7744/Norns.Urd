using Microsoft.Extensions.DependencyInjection;
using Moq;
using Norns.Urd;
using Norns.Urd.Reflection;
using System;
using Xunit;

namespace Test.Norns.Urd.DependencyInjection
{
    [AddSixInterceptor]
    public interface IGenericTest<T, R> : IDisposable
    {
        T F { get; set; }

        T F2 { get; set; }

        R GetR();

        R GetR(R r);

        [AddSixInterceptor]
        T GetT();

        T this[R index] { get; set; }

        C GetT<C>();
    }

    public interface ISealedGenericTest<T, R> 
    {
    }

    public sealed class SealedGenericTest<T, R> : ISealedGenericTest<T, R>
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    internal class InternalGenericTest<T, R> : ISealedGenericTest<T, R>
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class GenericTest<T, R> : IGenericTest<T, R>
    {
        public virtual T F { get; set; }
        public T F2 { get; set; }

        public T this[R index] { get => F; set => F = value; }

        public virtual R GetR() => default;

        public virtual R GetR(R r) => r;

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public T GetT() => default;

        public C GetT<C>() => default;
    }

    public abstract class AbstractGenericTest<T, R> : IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    [NonAspect]
    public interface INonAspectGenericTest<T, R> //: IDisposable
    {
    }

    public class SubGenericTest<T, R> : AbstractGenericTest<T, R>
    {
    }

    public class AopExtensionsTest
    {
        [Fact]
        public void WhenNonAspectGenericTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<INonAspectGenericTest<int, long>>(i => new Mock<INonAspectGenericTest<int, long>>().Object))
                .GetRequiredService<INonAspectGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.False(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.Null(pt.CreateServiceProviderGetter());
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenGenericOnlyInterfaceTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IGenericTest<int, long>>())
                 .GetRequiredService<IGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenGenericOnlyAbstractGenericClassTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<AbstractGenericTest<int, long>>())
                 .GetRequiredService<AbstractGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenGenericTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IGenericTest<int, long>, GenericTest<int, long>>())
                 .GetRequiredService<IGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p); 
            Assert.Equal(10, p.GetT());
            Assert.Equal(0, p.GetT<long>());
            Assert.Equal(10, p.GetT<int>());
        }

        [Fact]
        public void WhenOpenGenericTest()
        {
            var provider = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient(typeof(IGenericTest<,>), typeof(GenericTest<,>)));
            var p = provider.GetRequiredService<IGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p);
            p.F = 666;
            Assert.Equal(676, p.F);
            p.F = 777;
            Assert.Equal(787, p.F);
            Assert.Equal(0L, p.GetR());
            Assert.Equal(1L, p.GetR(1L));
            Assert.Equal(10, p.GetT());
            Assert.Equal(787, p.F);
            p[3L] = 878;
            Assert.Equal(898, p[8L]);
            Assert.Equal(888, p.F);

            var p2 = provider.GetRequiredService<IGenericTest<bool, double>>();
            Assert.False(p2.GetT());
            Assert.False(p2.F);
            p2.F = true;
            Assert.True(p2.F);
            Assert.Equal(10.0, p2.GetR());
            Assert.Equal(23.1, p2.GetR(13.1));
            p2[3L] = false;
            Assert.False(p2[4L]);
            Assert.False(p2.F);
        }

        [Fact]
        public void WhenGenericImplementationFactoryTest()
        {
            var provider = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddSingleton<IGenericTest<int, long>>(new GenericTest<int, long>()));
            var p = provider.GetRequiredService<IGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(pt.CreateInstanceGetter()(p));
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p);
            p.F = 666;
            Assert.Equal(682, p.F);
            p.F = 777;
            Assert.Equal(793, p.F);
            Assert.Equal(0L, p.GetR());
            Assert.Equal(1L, p.GetR(1L));
            Assert.Equal(16, p.GetT());
            Assert.Equal(793, p.F);
            p[3L] = 878;
            Assert.Equal(894, p[8L]);
            Assert.Equal(894, p.F);
        }

        [Fact]
        public void WhenGenericImplementationTypeTest()
        {
            var provider = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddSingleton<IGenericTest<int, long>, GenericTest<int, long>>());
            var p = provider.GetRequiredService<IGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p);
            p.F2 = 666;
            Assert.Equal(676, p.F2);
            p.F = 666;
            Assert.Equal(676, p.F);
            p.F = 777;
            Assert.Equal(787, p.F);
            Assert.Equal(0L, p.GetR());
            Assert.Equal(1L, p.GetR(1L));
            Assert.Equal(10, p.GetT());
            Assert.Equal(787, p.F);
            p[3L] = 878;
            Assert.Equal(898, p[8L]);
            Assert.Equal(888, p.F);
        }

        [Fact]
        public void WhenSealedOpenGenericTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient(typeof(ISealedGenericTest<,>), typeof(SealedGenericTest<,>)))
                 .GetRequiredService<ISealedGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.False(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.Null(pt.CreateServiceProviderGetter());
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenSealedGenericImplementationFactoryTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<ISealedGenericTest<int, long>>(i => new SealedGenericTest<int, long>()))
                .GetRequiredService<ISealedGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(pt.CreateInstanceGetter()(pt));
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenSealedGenericImplementationTypeTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<ISealedGenericTest<int, long>, SealedGenericTest<int, long>>())
                .GetRequiredService<ISealedGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(pt.CreateInstanceGetter()(pt));
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenInternalOpenGenericTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient(typeof(ISealedGenericTest<,>), typeof(InternalGenericTest<,>)))
                 .GetRequiredService<ISealedGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.False(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.Null(pt.CreateServiceProviderGetter());
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenInternalGenericImplementationFactoryTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<ISealedGenericTest<int, long>>(i => new InternalGenericTest<int, long>()))
                .GetRequiredService<ISealedGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(pt.CreateInstanceGetter()(pt));
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenInternalGenericImplementationTypeTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<ISealedGenericTest<int, long>, InternalGenericTest<int, long>>())
                .GetRequiredService<ISealedGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(pt.CreateInstanceGetter()(pt));
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenAbstractOpenGenericTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient(typeof(AbstractGenericTest<,>), typeof(SubGenericTest<,>)))
                 .GetRequiredService<AbstractGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenAbstractGenericImplementationFactoryTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<AbstractGenericTest<int, long>>(i => new SubGenericTest<int, long>()))
                .GetRequiredService<AbstractGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.NotNull(pt.CreateInstanceGetter()(pt));
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p);
        }

        [Fact]
        public void WhenAbstractGenericImplementationTypeTest()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<AbstractGenericTest<int, long>, SubGenericTest<int, long>>())
                .GetRequiredService<AbstractGenericTest<int, long>>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Null(pt.CreateInstanceGetter());
            Assert.NotNull(pt.CreateServiceProviderGetter()(p));
            Assert.NotNull(p);
        }
    }
}