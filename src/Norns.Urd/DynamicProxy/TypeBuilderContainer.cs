using System;
using System.Collections.Generic;
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
        }

        public ModuleBuilder ModuleBuilder { get; }

        public TypeBuilder TypeBuilder { get; }

        public Dictionary<string, FieldBuilder> Fields { get; }

        public Type Complete()
        {
            return TypeBuilder.CreateType();
        }
    }
}