using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.DynamicProxy
{
    public readonly struct TypeBuilderContainer
    {
        public TypeBuilderContainer(ModuleBuilder moduleBuilder, TypeBuilder typeBuilder)
        {
            ModuleBuilder = moduleBuilder;
            TypeBuilder = typeBuilder;
            Fields = new Dictionary<string, FieldBuilder>();
            PropertyInject = InitPropertyInject(typeBuilder);
        }

        public ModuleBuilder ModuleBuilder { get; }

        public TypeBuilder TypeBuilder { get; }

        public Dictionary<string, FieldBuilder> Fields { get; }

        public (ILGenerator il, MethodBuilder setter) PropertyInject { get; }

        public Type Complete()
        {
            PropertyInject.il.Emit(OpCodes.Ret);
            return TypeBuilder.CreateType();
        }

        private static (ILGenerator il, MethodBuilder setter) InitPropertyInject(TypeBuilder typeBuilder)
        {
            var propertyBuilder = typeBuilder.DefineProperty(Constants.ServiceProviderProperty, PropertyAttributes.None, typeof(IServiceProvider), Type.EmptyTypes);
            var setter = typeBuilder.DefineMethod($"Set_{Constants.ServiceProviderProperty}", MethodAttributes.Private, CallingConventions.Standard, null, Constants.DefaultConstructorParameters);
            var gil = setter.GetILGenerator();
            propertyBuilder.SetSetMethod(setter);
            return (gil, setter);
        }
    }
}