using Microsoft.Extensions.DependencyInjection;
using Norns.Urd;
using Norns.Urd.IOC;
using System;
using System.Linq;
using Xunit;

namespace Baymax.Test.AOP.Norns.Test
{
    public interface IIocTestInterface
    {
    }
}

namespace Baymax.Test.AOP.Norns
{
    public partial class IocTestClassPartial
    {
    }
}

namespace Baymax.Test.AOP.Norns
{
    public interface IIocTestInterface
    {
    }

    internal interface IIocTestInterfaceInternal
    {
    }

    internal class IocTestClassInternal : IIocTestInterface
    {
    }

    public class IocTestClass : IIocTestInterface
    {
    }

    public partial class IocTestClassPartial : IIocTestInterface
    {
    }

    public abstract class IocTestClassAbstract : IIocTestInterface
    {
    }

    public sealed class SealedClass : IIocTestInterface
    {
    }

    public class IocTest_Empty
    {
        private static IProxyServiceDescriptorConverter Converter = AopExtensions.InitProxyServiceDescriptorConverter();

        public interface INestedIocTestInterface
        {
        }

        public class NestedIocTestClass
        {
        }

        public class InternalInterfaceTest
        {
            public class WhenSingleton
            {
                [Fact]
                public void WhenAddTService()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<IIocTestInterfaceInternal>()
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IIocTestInterfaceInternal) == descriptor.ServiceType);
                    Assert.Equal("IIocTestInterfaceInternal", descriptor.ImplementationType.Name);
                    Assert.Throws<ArgumentException>(() => services.BuildServiceProvider()
                        .GetRequiredService<IIocTestInterfaceInternal>());
                }
            }
        }

        public class InterfaceTest
        {
            public class WhenSingleton
            {
                [Fact]
                public void WhenAddTService()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<IIocTestInterface>()
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IIocTestInterface) == descriptor.ServiceType);
                    Assert.Equal("IIocTestInterface_Proxy_Inherit", descriptor.ImplementationType.Name);
                    Assert.NotNull(services.BuildServiceProvider()
                        .GetRequiredService<IIocTestInterface>());
                }

                [Fact]
                public void WhenAddTServiceAndDifferentNamespaceAndSameName()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<Test.IIocTestInterface>()
                        .AddSingleton<IIocTestInterface>()
                        .ConfigureAop(converter: Converter);
                    Assert.Equal(3, services.Count);
                    var descriptor = services.First();
                    Assert.True(typeof(Test.IIocTestInterface) == descriptor.ServiceType);
                    Assert.Equal("IIocTestInterface_Proxy_Inherit", descriptor.ImplementationType.Name);
                    Assert.NotNull(services.BuildServiceProvider()
                        .GetRequiredService<IIocTestInterface>());
                }
            }

            public class WhenScoped
            {
                [Fact]
                public void WhenAddTService()
                {
                    var services = new ServiceCollection()
                        .AddScoped<IIocTestInterface>()
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IIocTestInterface) == descriptor.ServiceType);
                    Assert.Equal("IIocTestInterface_Proxy_Inherit", descriptor.ImplementationType.Name);
                    Assert.NotNull(services.BuildServiceProvider()
                        .GetRequiredService<IIocTestInterface>());
                }

                [Fact]
                public void WhenAddTServiceAndDifferentNamespaceAndSameName()
                {
                    var services = new ServiceCollection()
                        .AddScoped<Test.IIocTestInterface>()
                        .AddScoped<IIocTestInterface>()
                        .ConfigureAop(converter: Converter);
                    Assert.Equal(3, services.Count);
                    var descriptor = services.First();
                    Assert.True(typeof(Test.IIocTestInterface) == descriptor.ServiceType);
                    Assert.Equal("IIocTestInterface_Proxy_Inherit", descriptor.ImplementationType.Name);
                    Assert.NotNull(services.BuildServiceProvider()
                        .GetRequiredService<IIocTestInterface>());
                }
            }

            public class WhenTransient
            {
                [Fact]
                public void WhenAddTService()
                {
                    var services = new ServiceCollection()
                        .AddTransient<IIocTestInterface>()
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IIocTestInterface) == descriptor.ServiceType);
                    Assert.Equal("IIocTestInterface_Proxy_Inherit", descriptor.ImplementationType.Name);
                    Assert.NotNull(services.BuildServiceProvider()
                        .GetRequiredService<IIocTestInterface>());
                }

                [Fact]
                public void WhenAddTServiceAndDifferentNamespaceAndSameName()
                {
                    var services = new ServiceCollection()
                        .AddTransient<Test.IIocTestInterface>()
                        .AddTransient<IIocTestInterface>()
                        .ConfigureAop(converter: Converter);
                    Assert.Equal(3, services.Count);
                    var descriptor = services.First();
                    Assert.True(typeof(Test.IIocTestInterface) == descriptor.ServiceType);
                    Assert.Equal("IIocTestInterface_Proxy_Inherit", descriptor.ImplementationType.Name);
                    Assert.NotNull(services.BuildServiceProvider()
                        .GetRequiredService<IIocTestInterface>());
                }
            }
        }

        public class NotPublicClassTest
        {
            public class WhenSingleton
            {
                [Fact]
                public void WhenAddTService()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<IIocTestInterface, IocTestClassInternal>()
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IIocTestInterface) == descriptor.ServiceType);
                    var instance = services.BuildServiceProvider()
                       .GetRequiredService<IIocTestInterface>();
                    Assert.Equal("IIocTestInterface_Proxy_Facade", instance.GetType().Name);
                }
            }
        }

        public class NestedClassTest
        {
            public class WhenSingleton
            {
                [Fact]
                public void WhenAddTService()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<NestedIocTestClass>()
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(NestedIocTestClass) == descriptor.ServiceType);
                    Assert.Equal("IocTest_Empty\\+NestedIocTestClass_Proxy_Inherit", descriptor.ImplementationType.Name);
                    Assert.NotNull(services.BuildServiceProvider()
                        .GetRequiredService<NestedIocTestClass>());
                }
            }
        }

        public class NestedInterfaceTest
        {
            public class WhenSingleton
            {
                [Fact]
                public void WhenAddTService()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<INestedIocTestInterface>()
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(INestedIocTestInterface) == descriptor.ServiceType);
                    Assert.Equal("IocTest_Empty\\+INestedIocTestInterface_Proxy_Inherit", descriptor.ImplementationType.Name);
                    Assert.NotNull(services.BuildServiceProvider()
                        .GetRequiredService<INestedIocTestInterface>());
                }
            }
        }

        public class AbstractClassTest
        {
            public class WhenSingleton
            {
                [Fact]
                public void WhenAddTService()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<IocTestClassAbstract>()
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IocTestClassAbstract) == descriptor.ServiceType);
                    Assert.Equal("IocTestClassAbstract_Proxy_Inherit", descriptor.ImplementationType.Name);
                    Assert.NotNull(services.BuildServiceProvider()
                        .GetRequiredService<IocTestClassAbstract>());
                }
            }
        }

        public class PartialClassTest
        {
            public class WhenSingleton
            {
                [Fact]
                public void WhenAddTService()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<IocTestClassPartial>()
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IocTestClassPartial) == descriptor.ServiceType);
                    Assert.Equal("IocTestClassPartial_Proxy_Inherit", descriptor.ImplementationType.Name);
                    Assert.NotNull(services.BuildServiceProvider()
                        .GetRequiredService<IocTestClassPartial>());
                }
            }
        }

        public class ClassTest
        {
            public class WhenSingleton
            {
                [Fact]
                public void WhenAddTService()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<IocTestClass>()
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IocTestClass) == descriptor.ServiceType);
                    Assert.Equal("IocTestClass_Proxy_Inherit", descriptor.ImplementationType.Name);
                    Assert.NotNull(services.BuildServiceProvider()
                        .GetRequiredService<IocTestClass>());
                }

                [Fact]
                public void WhenAddTImplementation()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<IIocTestInterface, IocTestClass>()
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IIocTestInterface) == descriptor.ServiceType);
                    Assert.Equal("IocTestClass_Proxy_Inherit", descriptor.ImplementationType.Name);
                    Assert.NotNull(services.BuildServiceProvider()
                        .GetRequiredService<IIocTestInterface>());
                }

                [Fact]
                public void WhenAddImplementationFactory()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<IIocTestInterface>(i => new IocTestClass())
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IIocTestInterface) == descriptor.ServiceType);
                    var instance = services.BuildServiceProvider()
                        .GetRequiredService<IIocTestInterface>();
                    Assert.Equal("IIocTestInterface_Proxy_Facade", instance.GetType().Name);
                }

                [Fact]
                public void WhenAddImplementationInstance()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<IIocTestInterface>(new IocTestClass())
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IIocTestInterface) == descriptor.ServiceType);
                    var instance = services.BuildServiceProvider()
                        .GetRequiredService<IIocTestInterface>();
                    Assert.Equal("IIocTestInterface_Proxy_Facade", instance.GetType().Name);
                }
            }
        }

        public class SealedClassTest
        {
            public class WhenSingleton
            {
                [Fact]
                public void WhenAddTService()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<SealedClass>()
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(SealedClass) == descriptor.ServiceType);
                    Assert.Equal("SealedClass", descriptor.ImplementationType.Name);
                    Assert.NotNull(services.BuildServiceProvider()
                        .GetRequiredService<SealedClass>());
                }

                [Fact]
                public void WhenAddTImplementation()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<IIocTestInterface, SealedClass>()
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IIocTestInterface) == descriptor.ServiceType);
                    var instance = services.BuildServiceProvider()
                        .GetRequiredService<IIocTestInterface>();
                    Assert.Equal("IIocTestInterface_Proxy_Facade", instance.GetType().Name);
                }

                [Fact]
                public void WhenAddImplementationFactory()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<IIocTestInterface>(i => new SealedClass())
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IIocTestInterface) == descriptor.ServiceType);
                    var instance = services.BuildServiceProvider()
                        .GetRequiredService<IIocTestInterface>();
                    Assert.Equal("IIocTestInterface_Proxy_Facade", instance.GetType().Name);
                }

                [Fact]
                public void WhenAddImplementationInstance()
                {
                    var services = new ServiceCollection()
                        .AddSingleton<IIocTestInterface>(new SealedClass())
                        .ConfigureAop(converter: Converter);
                    
                    var descriptor = services.First();
                    Assert.True(typeof(IIocTestInterface) == descriptor.ServiceType);
                    var instance = services.BuildServiceProvider()
                        .GetRequiredService<IIocTestInterface>();
                    Assert.Equal("IIocTestInterface_Proxy_Facade", instance.GetType().Name);
                }
            }
        }
    }
}