using Microsoft.Extensions.DependencyInjection;
using Moq;
using Norns.Urd.Reflection;
using System;
using Xunit;

namespace Norns.Urd.Test.DependencyInjection
{
    //todo : facade support interfaces
    //todo : method, Property
    //todo : Property inject
    //todo ：Interceptor, NonAspectAttribute filter
    //todo : api start test
    public interface IGenericTest<T, R> //: IDisposable
    {
        T F { get; set; }
    }

    public interface ISealedGenericTest<T, R> //: IDisposable
    {
        //T F { get; }
    }

    public sealed class SealedGenericTest<T, R> : ISealedGenericTest<T, R>
    {
        //public T F => default;
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    internal class InternalGenericTest<T, R> : ISealedGenericTest<T, R>
    {
        //public virtual T F { get; set; }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public class GenericTest<T, R> : IGenericTest<T, R>
    {
        public virtual T F { get; set; }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class AbstractGenericTest<T, R> : IDisposable
    {
        //T F { get; }
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }

    [NonAspect]
    public interface INonAspectGenericTest<T, R> //: IDisposable
    {
        //T F { get; }
    }

    public class SubGenericTest<T, R> : AbstractGenericTest<T, R>
    {
        //T F { get; }
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
            //Assert.NotNull(p as IDisposable);
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
            //Assert.NotNull(p as IDisposable);
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
            //Assert.NotNull(p as IDisposable);
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
            //Assert.NotNull(p as IDisposable);
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
            var p2 = provider.GetRequiredService<IGenericTest<bool, long>>();
            Assert.False(p2.F);
            p2.F = true;
            Assert.True(p2.F);
            //Assert.NotNull(p as IDisposable);
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
            //Assert.NotNull(p as IDisposable);
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
            //Assert.NotNull(p as IDisposable);
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
            //Assert.NotNull(p as IDisposable);
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
            //Assert.NotNull(p as IDisposable);
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
            //Assert.NotNull(p as IDisposable);
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
            //Assert.NotNull(p as IDisposable);
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
            //Assert.NotNull(p as IDisposable);
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
            //Assert.NotNull(p as IDisposable);
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
            //Assert.NotNull(p as IDisposable);
        }
    }
}