using Norns.Urd.DynamicProxy;
using Norns.Urd.Interceptors.Features;
using Norns.Urd.Reflection;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd
{
    public static class AopInitializer
    {
        public static IProxyGenerator Init(this Action<IAspectConfiguration> config)
        {
            var configuration = new AspectConfiguration();
            configuration.AddParameterInject();
            config?.Invoke(configuration);
            return new ProxyGenerator(configuration);
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

        public static Func<object, IServiceProvider> CreateServiceProviderGetter(this Type type)
        {
            var field = type.GetField(Constants.ServiceProvider, BindingFlags.NonPublic | BindingFlags.Instance);
            return field?.CreateGetter<object, IServiceProvider>();
        }
    }
}