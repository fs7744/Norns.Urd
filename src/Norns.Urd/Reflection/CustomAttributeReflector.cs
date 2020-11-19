using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.Reflection
{
    public class CustomAttributeReflector
    {
        public CustomAttributeReflector(CustomAttributeData data)
        {
            Data = data;
            Tokens = GetAttrTokens(data.AttributeType);
            Invoke = CreateInvoker(data);
        }

        public CustomAttributeData Data { get; }

        public HashSet<RuntimeTypeHandle> Tokens { get; }

        public Func<Attribute> Invoke { get; }

        private static HashSet<RuntimeTypeHandle> GetAttrTokens(Type attributeType)
        {
            var tokenSet = new HashSet<RuntimeTypeHandle>();
            for (var attr = attributeType; attr != typeof(object); attr = attr.GetTypeInfo().BaseType)
            {
                tokenSet.Add(attr.TypeHandle);
            }
            return tokenSet;
        }

        private static Func<Attribute> CreateInvoker(CustomAttributeData data)
        {
            var attributeType = data.AttributeType;
            var dynamicMethod = new DynamicMethod($"invoker-{Guid.NewGuid():N}", typeof(Attribute), null, attributeType.GetTypeInfo().Module, true);
            var il = dynamicMethod.GetILGenerator();

            foreach (var constructorParameter in data.ConstructorArguments)
            {
                if (constructorParameter.ArgumentType.IsArray)
                {
                    il.EmitArray(((IEnumerable<CustomAttributeTypedArgument>)constructorParameter.Value).
                        Select(x => x.Value).ToArray(),
                        constructorParameter.ArgumentType.GetTypeInfo().UnWrapArrayType());
                }
                else
                {
                    il.EmitConstant(constructorParameter.Value, constructorParameter.ArgumentType);
                }
            }

            var attributeLocal = il.DeclareLocal(attributeType);

            il.Emit(OpCodes.Newobj, data.Constructor);

            il.Emit(OpCodes.Stloc, attributeLocal);

            var attributeTypeInfo = attributeType.GetTypeInfo();

            foreach (var namedArgument in data.NamedArguments)
            {
                il.Emit(OpCodes.Ldloc, attributeLocal);
                if (namedArgument.TypedValue.ArgumentType.IsArray)
                {
                    il.EmitArray(((IEnumerable<CustomAttributeTypedArgument>)namedArgument.TypedValue.Value).
                        Select(x => x.Value).ToArray(),
                        namedArgument.TypedValue.ArgumentType.GetTypeInfo().UnWrapArrayType());
                }
                else
                {
                    il.EmitConstant(namedArgument.TypedValue.Value, namedArgument.TypedValue.ArgumentType);
                }
                if (namedArgument.IsField)
                {
                    var field = attributeTypeInfo.GetField(namedArgument.MemberName);
                    il.Emit(OpCodes.Stfld, field);
                }
                else
                {
                    var property = attributeTypeInfo.GetProperty(namedArgument.MemberName);
                    il.Emit(OpCodes.Callvirt, property.SetMethod);
                }
            }
            il.Emit(OpCodes.Ldloc, attributeLocal);
            il.Emit(OpCodes.Ret);
            return (Func<Attribute>)dynamicMethod.CreateDelegate(typeof(Func<Attribute>));
        }


        internal static CustomAttributeReflector Create(CustomAttributeData customAttributeData)
        {
            return ReflectorCache<CustomAttributeData, CustomAttributeReflector>.GetOrAdd(customAttributeData, data => new CustomAttributeReflector(data));
        }
    }
}