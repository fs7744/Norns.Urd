using Norns.Urd.Extensions;
using Norns.Urd.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.Proxy
{
    public class FacadeProxyGenerator : IProxyGenerator
    {
        protected const string Instance = "instance";
        protected const string GeneratedNameSpace = "Norns.Urd.DynamicGenerated";
        public virtual ProxyTypes ProxyType { get; } = ProxyTypes.Facade;

        public string GetProxyTypeName(Type serviceType)
        {
            return $"{GeneratedNameSpace}.{serviceType.FullName}_Proxy_{ProxyType}";
        }

        public virtual Type CreateProxyType(ProxyGeneratorContext context)
        {
            DefineType(context);
            DefineFields(context);
            DefineConstructors(context);
            DefineMethods(context);
            context.AssistStaticTypeBuilder.CreateType();
            return context.TypeBuilder.CreateTypeInfo().AsType();
        }

        public void DefineMethods(ProxyGeneratorContext context)
        {
            foreach (var method in context.ServiceType.GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => !x.IsPropertyBinding()))
            {
                DefineMethod(context, method);
            }
        }

        public virtual void DefineMethod(ProxyGeneratorContext context, MethodInfo method)
        {
            var parameters = method.GetParameters().Select(i => i.ParameterType).ToArray();
            var mf = context.AssistStaticTypeBuilder.DefineMethodInfo(method, ProxyType);
            MethodBuilder methodBuilder = context.TypeBuilder.DefineMethod(method.Name, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, parameters);
            var il = methodBuilder.GetILGenerator();
            var caller = context.AssistStaticTypeBuilder.Fields[$"cm_{method.Name}"];
            il.Emit(OpCodes.Ldsfld, caller);
            il.Emit(OpCodes.Ldsfld, mf);
            il.EmitThis();
            il.Emit(OpCodes.Ldc_I4_0);

            var argsLocal = il.DeclareLocal(typeof(object[]));
            il.EmitInt(parameters.Length);
            il.Emit(OpCodes.Newarr, typeof(object));
            for (var i = 0; i < parameters.Length; i++)
            {
                il.Emit(OpCodes.Dup);
                il.EmitInt(i);
                il.EmitLoadArg(i + 1);
                if (parameters[i].IsByRef)
                {
                    il.EmitLdRef(parameters[i]);
                    il.EmitConvertToObject(parameters[i].GetElementType());
                }
                else
                {
                    il.EmitConvertToObject(parameters[i]);
                }
                il.Emit(OpCodes.Stelem_Ref);
            }
            il.Emit(OpCodes.Stloc, argsLocal);
            il.Emit(OpCodes.Ldloc, argsLocal);
            il.Emit(OpCodes.Newobj, context.ConstantInfo.AspectContextCtor);
            if (method.IsVoid())
            {
                il.Emit(OpCodes.Call, context.ConstantInfo.Invoke);
            }
            else
            {
                var c = il.DeclareLocal(context.ConstantInfo.AspectContextType);
                il.Emit(OpCodes.Stloc, c);
                il.Emit(OpCodes.Ldloc, c);
                il.Emit(OpCodes.Call, context.ConstantInfo.Invoke);
                il.Emit(OpCodes.Ldloc, c);
                il.Emit(OpCodes.Call, context.ConstantInfo.GetReturnValue);
                il.EmitConvertFromObject(method.ReturnType);
            }
            //for (var i = 0; i < parameters.Length; i++)
            //{
            //    if (parameters[i].IsByRef)
            //    {
            //        il.EmitLoadArg(i);
            //        il.Emit(OpCodes.Ldloc, argsLocal);
            //        il.EmitInt(i);
            //        il.Emit(OpCodes.Ldelem_Ref);
            //        il.EmitConvertFromObject(parameters[i].GetElementType());
            //        il.EmitStRef(parameters[i]);
            //    }
            //}
            il.Emit(OpCodes.Ret);
        }

        public virtual void DefineType(ProxyGeneratorContext context)
        {
            var serviceType = context.ServiceType;
            var (parent, interfaceTypes) = serviceType.IsInterface
                    ? (typeof(object), new Type[] { serviceType })
                    : (serviceType, Type.EmptyTypes);
            context.TypeBuilder = context.ModuleBuilder.DefineType(context.ProxyTypeName, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, parent, interfaceTypes);

            context.AssistStaticTypeBuilder.ModuleBuilder = context.ModuleBuilder;
            context.AssistStaticTypeBuilder.TypeBuilder = context.ModuleBuilder.DefineType($"{context.ProxyTypeName}_Assist", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, typeof(object));
        }

        public virtual void DefineFields(ProxyGeneratorContext context)
        {
            context.Fields.Add(Instance, context.TypeBuilder.DefineField(Instance, context.ServiceType, FieldAttributes.Private));
        }

        public virtual void DefineConstructors(ProxyGeneratorContext context)
        {
            var constructorInfos = context.ServiceType.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic && (c.IsPublic || c.IsFamily || c.IsFamilyAndAssembly || c.IsFamilyOrAssembly)).ToArray();
            if (constructorInfos.Length == 0)
            {
                var constructorBuilder = context.TypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, null);

                var ilGen = constructorBuilder.GetILGenerator();
                ilGen.EmitThis();
                ilGen.Emit(OpCodes.Call, context.ConstantInfo.ObjectCtor);
                ilGen.Emit(OpCodes.Ret);
            }
            else
            {
                foreach (var constructor in constructorInfos)
                {
                    Type[] parameterTypes = constructor.GetParameters().Select(i => i.ParameterType).ToArray();
                    var constructorBuilder = context.TypeBuilder.DefineConstructor(context.ServiceType.IsAbstract ? constructor.Attributes | MethodAttributes.Public : constructor.Attributes, constructor.CallingConvention, parameterTypes);
                    foreach (var customAttributeData in constructor.CustomAttributes)
                    {
                        constructorBuilder.SetCustomAttribute(DefineCustomAttribute(customAttributeData));
                    }
                    DefineParameters(constructor, constructorBuilder);

                    var ilGen = constructorBuilder.GetILGenerator();

                    ilGen.EmitThis();
                    for (var i = 1; i <= parameterTypes.Length; i++)
                    {
                        ilGen.EmitLoadArg(i);
                    }
                    ilGen.Emit(OpCodes.Call, constructor);

                    ilGen.Emit(OpCodes.Ret);
                }
            }
        }

        internal static void DefineParameters(ConstructorInfo constructor, ConstructorBuilder constructorBuilder)
        {
            var parameters = constructor.GetParameters();
            if (parameters.Length > 0)
            {
                var paramOffset = 1;
                for (var i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    var parameterBuilder = constructorBuilder.DefineParameter(i + paramOffset, parameter.Attributes, parameter.Name);
                    if (parameter.HasDefaultValue)
                    {
                        parameterBuilder.SetConstant(parameter.DefaultValue);
                    }
                    foreach (var attribute in parameter.CustomAttributes)
                    {
                        parameterBuilder.SetCustomAttribute(DefineCustomAttribute(attribute));
                    }
                }
            }
        }

        private static object ReadAttributeValue(CustomAttributeTypedArgument argument)
        {
            var value = argument.Value;
            if (!argument.ArgumentType.GetTypeInfo().IsArray)
            {
                return value;
            }
            var arguments = ((IEnumerable<CustomAttributeTypedArgument>)value)
                .Select(m => m.Value)
                .ToArray();
            return arguments;
        }

        internal static CustomAttributeBuilder DefineCustomAttribute(CustomAttributeData customAttributeData)
        {
            if (customAttributeData.NamedArguments != null)
            {
                var attributeTypeInfo = customAttributeData.AttributeType.GetTypeInfo();
                var constructorArgs = customAttributeData.ConstructorArguments
                    .Select(ReadAttributeValue)
                    .ToArray();
                var namedProperties = customAttributeData.NamedArguments
                        .Where(n => !n.IsField)
                        .Select(n => attributeTypeInfo.GetProperty(n.MemberName))
                        .ToArray();
                var propertyValues = customAttributeData.NamedArguments
                         .Where(n => !n.IsField)
                         .Select(n => ReadAttributeValue(n.TypedValue))
                         .ToArray();
                var namedFields = customAttributeData.NamedArguments.Where(n => n.IsField)
                         .Select(n => attributeTypeInfo.GetField(n.MemberName))
                         .ToArray();
                var fieldValues = customAttributeData.NamedArguments.Where(n => n.IsField)
                         .Select(n => ReadAttributeValue(n.TypedValue))
                         .ToArray();
                return new CustomAttributeBuilder(customAttributeData.Constructor, constructorArgs
                   , namedProperties
                   , propertyValues, namedFields, fieldValues);
            }
            else
            {
                return new CustomAttributeBuilder(customAttributeData.Constructor,
                    customAttributeData.ConstructorArguments.Select(c => c.Value).ToArray());
            }
        }
    }
}