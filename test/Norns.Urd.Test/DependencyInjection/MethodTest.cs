using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Norns.Urd.Test.DependencyInjection
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

        //void VoidMethod();
    }

    public class MethodTest
    {
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

        //[Fact]
        //public void InterfaceWhenVoidMethod()
        //{
        //    var p = AopTestExtensions.ConfigServiceCollectionWithAop(i => i.AddTransient<IMTest>())
        //        .GetRequiredService<IMTest>();
        //    var pt = p.GetType();
        //    Assert.True(pt.IsProxyType());
        //    p.VoidMethod();
        //}
    }
}
