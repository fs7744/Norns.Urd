using Microsoft.Extensions.DependencyInjection;
using Norns.Urd.Attributes;
using Norns.Urd.DynamicProxy;
using Norns.Urd.Interceptors.Features;
using Norns.Urd.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;

namespace Norns.Urd
{
    public static class AopInitializer
    {
        public static (IProxyGenerator, IEnumerable<Action<IServiceCollection>>, IAspectConfiguration) Init(this Action<IAspectConfiguration> config)
        {
            var configuration = new AspectConfiguration();
            configuration.AddParameterInject();
            config?.Invoke(configuration);
            return (new ProxyGenerator(configuration), configuration.ConfigServices, configuration);
        }

        public static Func<object, object> CreateInstanceGetter(this Type type)
        {
            var field = type.GetField(Constants.Instance, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.CreateGetter<object, object>();
        }

        public static Action<object, object> CreateInstanceSetter(this Type type)
        {
            var field = type.GetField(Constants.Instance, BindingFlags.NonPublic | BindingFlags.Instance);
            if (field == null) return null;
            var p = type.GetProperty(Constants.ServiceProviderProperty, BindingFlags.NonPublic | BindingFlags.Instance);
            var fs = type.GetField(Constants.ServiceProvider, BindingFlags.NonPublic | BindingFlags.Instance);
            var method = new DynamicMethod(Guid.NewGuid().ToString("N"), typeof(void), new Type[] { typeof(object), typeof(object) });
            var il = method.GetILGenerator();
            il.EmitLoadArg(0);
            il.EmitLoadArg(1);
            il.EmitConvertObjectTo(field.FieldType);
            il.Emit(OpCodes.Stfld, field);
            if (fs != null)
            {
                il.EmitLoadArg(0);
                il.EmitLoadArg(0);
                il.Emit(OpCodes.Ldfld, fs);
                il.Emit(OpCodes.Call, p.SetMethod);
            }
            il.Emit(OpCodes.Ret);
            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }

        public static Func<IServiceProvider, object> CreateFacadeInstanceCreator(this Type type)
        {
            var constructor = type.GetConstructors().FirstOrDefault(i => i.IsPublic && i.IsDefined(typeof(DynamicProxyAttribute), false));
            if (constructor == null) return null;
            var method = new DynamicMethod(Guid.NewGuid().ToString("N"), typeof(object), Constants.DefaultConstructorParameters);
            var il = method.GetILGenerator();
            il.EmitLoadArg(0);
            il.Emit(OpCodes.Newobj, constructor);
            il.Emit(OpCodes.Ret);
            return (Func<IServiceProvider, object>)method.CreateDelegate(typeof(Func<IServiceProvider, object>));
        }

        public static Func<object, IServiceProvider> CreateServiceProviderGetter(this Type type)
        {
            var field = type.GetField(Constants.ServiceProvider, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.CreateGetter<object, IServiceProvider>();
        }

        public static AsyncAspectDelegate ConverTotReturnTask<T>(AsyncAspectDelegate aspectDelegate)
        {
            return c =>
            {
                var task = aspectDelegate(c).ContinueWith<T>((t, cc) =>
                {
                    if (t.Exception != null)
                    {
                        throw t.Exception.InnerException;
                    }
                    var r = (cc as AspectContext).ReturnValue as Task<T>;
                    return r.Result;
                }, c);
                return task;
            };
        }

        public static AsyncAspectDelegate ConverTotReturnValueTask<T>(AsyncAspectDelegate aspectDelegate)
        {
            return c =>
            {
                var task = aspectDelegate(c).ContinueWith<T>((t, cc) =>
                {
                    if (t.Exception != null)
                    {
                        throw t.Exception.InnerException;
                    }
                    var vt = cc as AspectContext;
                    var r = (ValueTask<T>)(vt).ReturnValue;
                    return r.Result;
                }, c);
                c.ReturnTask = new ValueTask<T>(task);
                return task;
            };
        }
    }
}