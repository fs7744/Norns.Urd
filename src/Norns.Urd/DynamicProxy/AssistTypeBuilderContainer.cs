using Norns.Urd.Interceptors;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.DynamicProxy
{
    public readonly struct AssistTypeBuilderContainer
    {
        public AssistTypeBuilderContainer(ModuleBuilder moduleBuilder, TypeBuilder typeBuilder)
        {
            ModuleBuilder = moduleBuilder;
            TypeBuilder = typeBuilder;
            Fields = new Dictionary<string, FieldBuilder>();
            InitMethod = TypeBuilder.DefineMethod("Init", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(void), new Type[] { typeof(IInterceptorConfiguration) });
            InitMethodIL = InitMethod.GetILGenerator();
        }

        public ModuleBuilder ModuleBuilder { get; }

        public TypeBuilder TypeBuilder { get; }

        public Dictionary<string, FieldBuilder> Fields { get; }

        public MethodBuilder InitMethod { get; }

        public ILGenerator InitMethodIL { get; }

        public Type Complete(IInterceptorConfiguration configuration)
        {
            InitMethodIL.Emit(OpCodes.Ret);
            var type = TypeBuilder.CreateType();
            type.GetMethod(InitMethod.Name).Invoke(null, new object[] { configuration });
            return type;
        }
    }
}