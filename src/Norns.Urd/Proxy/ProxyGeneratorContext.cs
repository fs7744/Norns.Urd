using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.Proxy
{
    public class ProxyGeneratorContext : ProxyTypeContext
    {
        public Type ServiceType { get; set; }

        public string ProxyTypeName { get; set; }

        public AssistProxyTypeContext AssistStaticTypeBuilder { get; } = new AssistProxyTypeContext();

        public IAspectConfiguration Configuration { get => AssistStaticTypeBuilder.Configuration; set => AssistStaticTypeBuilder.Configuration = value; }

        public IServiceProvider ServiceProvider { get => AssistStaticTypeBuilder.ServiceProvider; set => AssistStaticTypeBuilder.ServiceProvider = value; }
    }

    public class ProxyTypeContext
    {
        public ModuleBuilder ModuleBuilder { get; set; }

        public TypeBuilder TypeBuilder { get; set; }

        public Dictionary<string, FieldBuilder> Fields { get; } = new Dictionary<string, FieldBuilder>();
    }

    public class AssistProxyTypeContext : ProxyTypeContext
    {
        public ILGenerator ConstructorIL { get; set; }

        public IAspectConfiguration Configuration { get; set; }

        public IServiceProvider ServiceProvider { get; set; }

        public FieldBuilder GetInterceptorFactoryField()
        {
            if (!Fields.ContainsKey("fm_IInterceptorFactory"))
            {
                var field = TypeBuilder.DefineField("fm_IInterceptorFactory", typeof(IInterceptorFactory), FieldAttributes.Public | FieldAttributes.Static );
                Fields.Add(field.Name, field);
                var m = TypeBuilder.DefineMethod("Init", MethodAttributes.Public | MethodAttributes.Static , CallingConventions.Standard, typeof(void), new Type[] { typeof(IInterceptorFactory) });
                ConstructorIL = m.GetILGenerator();
            }
            return Fields["fm_IInterceptorFactory"];
        }

        public FieldBuilder DefineMethodInfo(MethodInfo serviceMethod, ProxyTypes proxyType)
        {
            var f = GetInterceptorFactoryField();
            var field = TypeBuilder.DefineField($"fm_{serviceMethod.Name}", typeof(MethodInfo), FieldAttributes.Static | FieldAttributes.Assembly);
            ConstructorIL.Emit(OpCodes.Ldtoken, serviceMethod);
            ConstructorIL.Emit(OpCodes.Ldtoken, serviceMethod.DeclaringType);
            ConstructorIL.Emit(OpCodes.Call, EmitExtensions.GetMethod<Func<RuntimeMethodHandle, RuntimeTypeHandle, MethodBase>>((h1, h2) => MethodBase.GetMethodFromHandle(h1, h2)));
            ConstructorIL.Emit(OpCodes.Castclass, typeof(MethodInfo));
            ConstructorIL.Emit(OpCodes.Stsfld, field);
            Fields.Add(field.Name, field);

            var cField = TypeBuilder.DefineField($"cm_{serviceMethod.Name}", typeof(AspectDelegate), FieldAttributes.Static | FieldAttributes.Assembly);
            ConstructorIL.EmitLoadArg(0);
            ConstructorIL.Emit(OpCodes.Ldsfld, field);
            ConstructorIL.Emit(ProxyTypes.Inherit == proxyType ? OpCodes.Ldc_I4_0 : OpCodes.Ldc_I4_1);
            ConstructorIL.Emit(OpCodes.Callvirt, typeof(IInterceptorFactory).GetMethod(nameof(IInterceptorFactory.GetInterceptor)));
            ConstructorIL.Emit(OpCodes.Stsfld, cField);
            Fields.Add(cField.Name, cField);

            return field;
        }

        public Type CreateType()
        {
            GetInterceptorFactoryField();
            ConstructorIL.Emit(OpCodes.Ret);
            var type = TypeBuilder.CreateTypeInfo().AsType();
            type.GetMethod("Init").Invoke(null, new object[] { ServiceProvider.GetRequiredService<IInterceptorFactory>() }); // todo:性能优化
            return type;
        }
    }
}