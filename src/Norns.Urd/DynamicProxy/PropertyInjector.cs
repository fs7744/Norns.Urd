using Norns.Urd.Reflection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.DynamicProxy
{
    public static class PropertyInjector
    {
        private readonly static ConcurrentDictionary<Type, Action<object, IServiceProvider>> cache = new ConcurrentDictionary<Type, Action<object, IServiceProvider>>();

        public static void Inject(object instance, IServiceProvider provider)
        {
            if (instance == null) return;
            cache.GetOrAdd(instance.GetType(), CreatePropertyInjector)(instance, provider);
        }

        private static Action<object, IServiceProvider> CreatePropertyInjector(Type type)
        {
            var method = new DynamicMethod(Guid.NewGuid().ToString("N"), typeof(void), new Type[] { typeof(object), typeof(IServiceProvider) });
            var il = method.GetILGenerator();
            var t = type.IsProxyType() && type.BaseType != typeof(object) ? type.BaseType : type;
            var ps = t.GetProperties(Constants.MethodBindingFlags)
                .Where(i => i.CanWrite && i.GetReflector().IsDefined<InjectAttribute>());
            foreach (var p in ps)
            {
                il.EmitLoadArg(0);
                il.EmitLoadArg(1);
                il.EmitType(p.PropertyType);
                il.Emit(OpCodes.Callvirt, Constants.GetServiceFromDI);
                il.EmitConvertObjectTo(p.PropertyType);
                il.Emit(OpCodes.Callvirt, p.SetMethod);
            }
            var fs = t.GetFields(Constants.MethodBindingFlags)
                .Where(i => i.GetReflector().IsDefined<InjectAttribute>());
            foreach (var f in fs)
            {
                il.EmitLoadArg(0);
                il.EmitLoadArg(1);
                il.EmitType(f.FieldType);
                il.Emit(OpCodes.Callvirt, Constants.GetServiceFromDI);
                il.EmitConvertObjectTo(f.FieldType);
                il.Emit(OpCodes.Stfld, f);
            }
            il.Emit(OpCodes.Ret);
            return (Action<object, IServiceProvider>)method.CreateDelegate(typeof(Action<object, IServiceProvider>));
        }
    }
}