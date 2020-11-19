using Norns.Urd.Interceptors;
using Norns.Urd.Reflection;
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
            InitMethod = TypeBuilder.DefineMethod("Init", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(void), new Type[] { typeof(IInterceptorCreator) });
            InitMethodIL = InitMethod.GetILGenerator();
        }

        public ModuleBuilder ModuleBuilder { get; }

        public TypeBuilder TypeBuilder { get; }

        public Dictionary<string, FieldBuilder> Fields { get; }

        public MethodBuilder InitMethod { get; }

        public ILGenerator InitMethodIL { get; }

        public Type Complete(IInterceptorCreator interceptorCreator)
        {
            InitMethodIL.Emit(OpCodes.Ret);
            var type = TypeBuilder.CreateType();
            type.GetMethod(InitMethod.Name).Invoke(null, new object[] { interceptorCreator });
            return type;
        }

        internal FieldBuilder DefineMethodInfoCache(MethodInfo method)
        {
            var field = TypeBuilder.DefineField($"{method.GetReflector().DisplayName}_cache", typeof(MethodInfo), FieldAttributes.Static | FieldAttributes.Assembly);
            InitMethodIL.EmitMethod(method);
            InitMethodIL.Emit(OpCodes.Stsfld, field);
            Fields.Add(field.Name, field);
            return field;
        }

        internal FieldBuilder DefineMethodInfoCaller(MethodInfo method, string name)
        {
            var isAsync = method.IsAsync();
            var cField = TypeBuilder.DefineField($"cm_{name}", isAsync ? typeof(AsyncAspectDelegate) : typeof(AspectDelegate), FieldAttributes.Static | FieldAttributes.Assembly);
            InitMethodIL.EmitLoadArg(0);
            InitMethodIL.EmitMethod(method);
            InitMethodIL.Emit(OpCodes.Callvirt, isAsync ? Constants.GetInterceptorAsync : Constants.GetInterceptor);
            InitMethodIL.Emit(OpCodes.Stsfld, cField);
            Fields.Add(cField.Name, cField);
            return cField;
        }

        internal FieldBuilder DefineOpenGenericMethodInfoCaller(MethodInfo method, string name)
        {
            var isAsync = method.IsAsync();
            var cField = TypeBuilder.DefineField($"cm_{name}", isAsync ? typeof(AsyncAspectDelegate) : typeof(AspectDelegate), FieldAttributes.Static | FieldAttributes.Assembly);
            InitMethodIL.EmitLoadArg(0);
            InitMethodIL.EmitString(name);
            InitMethodIL.Emit(OpCodes.Callvirt, isAsync ? Constants.GetOpenGenericInterceptorAsync : Constants.GetOpenGenericInterceptor);
            InitMethodIL.Emit(OpCodes.Stsfld, cField);
            Fields.Add(cField.Name, cField);
            return cField;
        }
    }
}