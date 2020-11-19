using Microsoft.Extensions.DependencyInjection;
using Norns.Urd;
using Norns.Urd.Reflection;
using Xunit;

namespace Test.Norns.Urd.DependencyInjection
{
    public interface IMTest
    {
        [NonAspect]
        void NonAspectVoidMethod();

        [NonAspect]
        int NonAspectIntMethod();

        [NonAspect]
        int NonAspectIntMethodOutParameter(out int y);

        [NonAspect]
        public int DefaultNonAspectIntMethodOutParameter(out int y)
        {
            y = 4;
            return 5;
        }

        int IntMethod();
    }

    public abstract class MTest
    {
        [NonAspect]
        public abstract void NonAspectVoidMethod();

        [NonAspect]
        public abstract int NonAspectIntMethod();

        [NonAspect]
        public abstract int NonAspectIntMethodOutParameter(out int y);

        [NonAspect]
        public virtual int DefaultNonAspectIntMethodOutParameter(out int y)
        {
            y = 4;
            return 5;
        }

        [NonAspect]
        public int NotVirtualDefaultNonAspectIntMethodOutParameter(out int y)
        {
            y = 4;
            return 5;
        }
    }

    public class NMTest : MTest
    {
        public override int NonAspectIntMethod()
        {
            return 7;
        }

        public override int NonAspectIntMethodOutParameter(out int y)
        {
            y = 6;
            return 7;
        }

        public override void NonAspectVoidMethod()
        {
        }
    }

    public class MMTest : IMTest
    {
        public int IntMethod() => 6;

        [NonAspect]
        public virtual int NonAspectIntMethod() => 3;

        [NonAspect]
        public virtual int NonAspectIntMethodOutParameter(out int y)
        {
            y = 4;
            return 5;
        }

        [NonAspect]
        public virtual void NonAspectVoidMethod()
        {
        }
    }

    public class MethodTest
    {
        #region Interface

        [Fact]
        public void InterfaceWhenNonAspectVoidMethod()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest>())
                .GetRequiredService<IMTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            p.NonAspectVoidMethod();
        }

        [Fact]
        public void InterfaceWhenNonAspectIntMethod()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest>())
                .GetRequiredService<IMTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(0, p.NonAspectIntMethod());
        }

        [Fact]
        public void InterfaceWhenNonAspectIntMethodOutParameter()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest>())
                .GetRequiredService<IMTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(0, p.NonAspectIntMethodOutParameter(out var y));
            Assert.Equal(0, y);
        }

        [Fact]
        public void InterfaceWhenDefaultNonAspectIntMethodOutParameter()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest>())
                .GetRequiredService<IMTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(5, p.DefaultNonAspectIntMethodOutParameter(out var y));
            Assert.Equal(4, y);
        }

        #endregion Interface

        #region abstract class

        [Fact]
        public void AbstractClassWhenNonAspectVoidMethod()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<MTest>())
                .GetRequiredService<MTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            p.NonAspectVoidMethod();
        }

        [Fact]
        public void AbstractClassWhenNonAspectIntMethod()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<MTest>())
                .GetRequiredService<MTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(0, p.NonAspectIntMethod());
        }

        [Fact]
        public void AbstractClassWhenNonAspectIntMethodOutParameter()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<MTest>())
                .GetRequiredService<MTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(0, p.NonAspectIntMethodOutParameter(out var y));
            Assert.Equal(0, y);
        }

        [Fact]
        public void AbstractClassWhenDefaultNonAspectIntMethodOutParameter()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<MTest>())
                .GetRequiredService<MTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(5, p.DefaultNonAspectIntMethodOutParameter(out var y));
            Assert.Equal(4, y);
        }

        [Fact]
        public void AbstractClassWhenNotVirtualDefaultNonAspectIntMethodOutParameter()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<MTest>())
                .GetRequiredService<MTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(5, p.NotVirtualDefaultNonAspectIntMethodOutParameter(out var y));
            Assert.Equal(4, y);
        }

        [Fact]
        public void ImplementationFactoryAbstractClassWhenNonAspectVoidMethod()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<MTest>(x => new NMTest()))
                .GetRequiredService<MTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            p.NonAspectVoidMethod();
        }

        [Fact]
        public void ImplementationFactoryAbstractClassWhenNonAspectIntMethod()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<MTest>(x => new NMTest()))
                .GetRequiredService<MTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(7, p.NonAspectIntMethod());
        }

        [Fact]
        public void ImplementationFactoryAbstractClassWhenNonAspectIntMethodOutParameter()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<MTest>(x => new NMTest()))
                .GetRequiredService<MTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(7, p.NonAspectIntMethodOutParameter(out var y));
            Assert.Equal(6, y);
        }

        [Fact]
        public void ImplementationFactoryAbstractClassWhenDefaultNonAspectIntMethodOutParameter()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<MTest>(x => new NMTest()))
                .GetRequiredService<MTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(5, p.DefaultNonAspectIntMethodOutParameter(out var y));
            Assert.Equal(4, y);
        }

        [Fact]
        public void ImplementationFactoryAbstractClassWhenNotVirtualDefaultNonAspectIntMethodOutParameter()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<MTest>(x => new NMTest()))
                .GetRequiredService<MTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(5, p.NotVirtualDefaultNonAspectIntMethodOutParameter(out var y));
            Assert.Equal(4, y);
        }

        #endregion abstract class

        #region normal class

        [Fact]
        public void ClassWhenNonAspectVoidMethod()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest, MMTest>())
                .GetRequiredService<IMTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            p.NonAspectVoidMethod();
        }

        [Fact]
        public void ClassWhenNonAspectIntMethod()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest, MMTest>())
                .GetRequiredService<IMTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(3, p.NonAspectIntMethod());
        }

        [Fact]
        public void ClassWhenNonAspectIntMethodOutParameter()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest, MMTest>())
                .GetRequiredService<IMTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(5, p.NonAspectIntMethodOutParameter(out var y));
            Assert.Equal(4, y);
        }

        [Fact]
        public void ClassWhenDefaultNonAspectIntMethodOutParameter()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest, MMTest>())
                .GetRequiredService<IMTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(5, p.DefaultNonAspectIntMethodOutParameter(out var y));
            Assert.Equal(4, y);
        }

        [Fact]
        public void ImplementationFactoryClassWhenNonAspectVoidMethod()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest>(x => new MMTest()))
                .GetRequiredService<IMTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            p.NonAspectVoidMethod();
        }

        [Fact]
        public void ImplementationFactoryClassWhenNonAspectIntMethod()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest>(x => new MMTest()))
                .GetRequiredService<IMTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(3, p.NonAspectIntMethod());
        }

        [Fact]
        public void ImplementationFactoryClassWhenNonAspectIntMethodOutParameter()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest>(x => new MMTest()))
                .GetRequiredService<IMTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(5, p.NonAspectIntMethodOutParameter(out var y));
            Assert.Equal(4, y);
        }

        [Fact]
        public void ImplementationFactoryClassWhenDefaultNonAspectIntMethodOutParameter()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest>(x => new MMTest()))
                .GetRequiredService<IMTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(5, p.DefaultNonAspectIntMethodOutParameter(out var y));
            Assert.Equal(4, y);
        }

        #endregion normal class

        [Fact]
        public void InterfaceWhenVoidMethod()
        {
            var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest>())
                .GetRequiredService<IMTest>();
            var pt = p.GetType();
            Assert.True(pt.IsProxyType());
            Assert.Equal(10, p.IntMethod());
        }
    }
}