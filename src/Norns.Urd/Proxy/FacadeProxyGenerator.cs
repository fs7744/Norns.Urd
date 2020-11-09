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
            DefineProperties(context);
            context.AssistStaticTypeBuilder.CreateType();
            return context.TypeBuilder.CreateTypeInfo().AsType();
        }

        public virtual void DefineProperties(ProxyGeneratorContext context)
        {
            foreach (var property in context.ServiceType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                DefineProperty(context, property);
            }
        }

        public virtual void DefineProperty(ProxyGeneratorContext context, PropertyInfo property)
        {
            var propertyBuilder = context.TypeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, Type.EmptyTypes);

            foreach (var customAttributeData in property.CustomAttributes)
            {
                propertyBuilder.SetCustomAttribute(DefineCustomAttribute(customAttributeData));
            }

            if (property.CanRead)
            {
                var method = DefineMethod(context, property.GetMethod);
                propertyBuilder.SetGetMethod(method);
            }
            if (property.CanWrite)
            {
                var method = DefineMethod(context, property.SetMethod);
                propertyBuilder.SetSetMethod(method);
            }
        }

        public void DefineMethods(ProxyGeneratorContext context)
        {
            foreach (var method in context.ServiceType.GetTypeInfo()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => !x.IsPropertyBinding() && !context.ConstantInfo.IgnoreMethods.Contains(x.Name)))
            {
                DefineMethod(context, method);
            }
        }

        public virtual void GetServiceInstance(ProxyGeneratorContext context, ILGenerator il)
        {
            il.EmitThis();
            il.Emit(OpCodes.Ldfld, context.Fields[ConstantInfo.Instance]);
        }

        public virtual MethodBuilder DefineMethod(ProxyGeneratorContext context, MethodInfo method)
        {
            var parameters = method.GetParameters().Select(i => i.ParameterType).ToArray();
            var mf = context.AssistStaticTypeBuilder.DefineMethodInfo(method, ProxyType);
            MethodBuilder methodBuilder = context.TypeBuilder.DefineMethod(method.Name, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, parameters);
            DefineGenericParameter(method, methodBuilder);
            var il = methodBuilder.GetILGenerator();
            var caller = context.AssistStaticTypeBuilder.Fields[$"cm_{method.Name}"];
            il.Emit(OpCodes.Ldsfld, caller);
            if (method.IsGenericMethodDefinition)
            {
                var gm = method.MakeGenericMethod(methodBuilder.GetGenericArguments());
                il.Emit(OpCodes.Ldtoken, gm);
                il.Emit(OpCodes.Ldtoken, gm.DeclaringType);
                il.Emit(OpCodes.Call, ConstantInfo.GetMethodFromHandle);
                il.Emit(OpCodes.Castclass, typeof(MethodInfo));
            }
            else
            {
                il.Emit(OpCodes.Ldsfld, mf);
            }
            GetServiceInstance(context, il);
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
            il.EmitThis();
            il.Emit(OpCodes.Ldfld, context.Fields[ConstantInfo.ServiceProvider]);
            il.Emit(OpCodes.Newobj, context.ConstantInfo.AspectContextCtor);
            if (method.IsVoid())
            {
                il.Emit(OpCodes.Call, context.ConstantInfo.Invoke);
            }
            else if (method.IsTask() || method.IsValueTask())
            {
                il.Emit(OpCodes.Call, context.ConstantInfo.InvokeAsync);
            }
            else if (method.IsReturnTask() || method.IsReturnValueTask())
            {
                var c = il.DeclareLocal(context.ConstantInfo.AspectContextType);
                il.Emit(OpCodes.Stloc, c);
                il.Emit(OpCodes.Ldloc, c);
                il.Emit(OpCodes.Call, context.ConstantInfo.InvokeAsync);
                il.Emit(OpCodes.Pop);
                il.Emit(OpCodes.Ldloc, c);
                il.Emit(OpCodes.Call, context.ConstantInfo.GetReturnValue);
                il.EmitConvertFromObject(method.ReturnType);
            }
            else
            {
                var c = il.DeclareLocal(context.ConstantInfo.AspectContextType);
                il.Emit(OpCodes.Stloc, c);
                il.Emit(OpCodes.Ldloc, c);
                il.Emit(OpCodes.Call, method.IsAsync() ? context.ConstantInfo.InvokeAsync : context.ConstantInfo.Invoke);
                il.Emit(OpCodes.Ldloc, c);
                il.Emit(OpCodes.Call, context.ConstantInfo.GetReturnValue);
                il.EmitConvertFromObject(method.ReturnType);
            }
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].IsByRef)
                {
                    il.EmitLoadArg(i + 1);
                    il.Emit(OpCodes.Ldloc, argsLocal);
                    il.EmitInt(i);
                    il.Emit(OpCodes.Ldelem_Ref);
                    il.EmitConvertFromObject(parameters[i].GetElementType());
                    il.EmitStRef(parameters[i]);
                }
            }
            il.Emit(OpCodes.Ret);
            return methodBuilder;
        }


        public static void DefineGenericParameter(Type targetType, TypeBuilder typeBuilder)
        {
            if (!targetType.GetTypeInfo().IsGenericTypeDefinition)
            {
                return;
            }
            var genericArguments = targetType.GetTypeInfo().GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
            var genericArgumentsBuilders = typeBuilder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
            for (var index = 0; index < genericArguments.Length; index++)
            {
                genericArgumentsBuilders[index].SetGenericParameterAttributes(ToClassGenericParameterAttributes(genericArguments[index].GenericParameterAttributes));
                foreach (var constraint in genericArguments[index].GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                {
                    if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                    if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                }
            }
        }

        public static GenericParameterAttributes ToClassGenericParameterAttributes(GenericParameterAttributes attributes)
        {
            if (attributes == GenericParameterAttributes.None)
            {
                return GenericParameterAttributes.None;
            }
            if (attributes.HasFlag(GenericParameterAttributes.SpecialConstraintMask))
            {
                return GenericParameterAttributes.SpecialConstraintMask;
            }
            if (attributes.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint))
            {
                return GenericParameterAttributes.NotNullableValueTypeConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) && attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                return GenericParameterAttributes.ReferenceTypeConstraint | GenericParameterAttributes.DefaultConstructorConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint))
            {
                return GenericParameterAttributes.ReferenceTypeConstraint;
            }
            if (attributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
            {
                return GenericParameterAttributes.DefaultConstructorConstraint;
            }
            return GenericParameterAttributes.None;
        }

        protected static void DefineGenericParameter(MethodInfo tergetMethod, MethodBuilder methodBuilder)
        {
            if (!tergetMethod.IsGenericMethod)
            {
                return;
            }
            var genericArguments = tergetMethod.GetGenericArguments().Select(t => t.GetTypeInfo()).ToArray();
            var genericArgumentsBuilders = methodBuilder.DefineGenericParameters(genericArguments.Select(a => a.Name).ToArray());
            for (var index = 0; index < genericArguments.Length; index++)
            {
                genericArgumentsBuilders[index].SetGenericParameterAttributes(genericArguments[index].GenericParameterAttributes);
                foreach (var constraint in genericArguments[index].GetGenericParameterConstraints().Select(t => t.GetTypeInfo()))
                {
                    if (constraint.IsClass) genericArgumentsBuilders[index].SetBaseTypeConstraint(constraint.AsType());
                    if (constraint.IsInterface) genericArgumentsBuilders[index].SetInterfaceConstraints(constraint.AsType());
                }
            }
        }

        public virtual void DefineType(ProxyGeneratorContext context)
        {
            var serviceType = context.ServiceType;
            var (parent, interfaceTypes) = serviceType.IsInterface
                    ? (typeof(object), new Type[] { serviceType })
                    : (serviceType, Type.EmptyTypes);
            context.TypeBuilder = context.ModuleBuilder.DefineType(context.ProxyTypeName, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, parent, interfaceTypes);
            DefineGenericParameter(serviceType, context.TypeBuilder);
            context.AssistStaticTypeBuilder.ModuleBuilder = context.ModuleBuilder;
            context.AssistStaticTypeBuilder.TypeBuilder = context.ModuleBuilder.DefineType($"{context.ProxyTypeName}_Assist", TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, typeof(object));
        }

        public virtual void DefineFields(ProxyGeneratorContext context)
        {
            context.Fields.Add(ConstantInfo.Instance, context.TypeBuilder.DefineField(ConstantInfo.Instance, context.ServiceType, FieldAttributes.Private));
            context.Fields.Add(ConstantInfo.ServiceProvider, context.TypeBuilder.DefineField(ConstantInfo.ServiceProvider, typeof(IServiceProvider), FieldAttributes.Private));
        }

        public virtual void DefineConstructors(ProxyGeneratorContext context)
        {
            var constructorInfos = context.ServiceType.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic && (c.IsPublic || c.IsFamily || c.IsFamilyAndAssembly || c.IsFamilyOrAssembly)).ToArray();
            if (constructorInfos.Length == 0)
            {
                var constructorBuilder = context.TypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new Type[] { typeof(IServiceProvider) });

                var ilGen = constructorBuilder.GetILGenerator();
                ilGen.EmitThis();
                ilGen.Emit(OpCodes.Call, context.ConstantInfo.ObjectCtor);
                ilGen.EmitThis();
                ilGen.EmitLoadArg(1);
                ilGen.Emit(OpCodes.Stfld, context.Fields[ConstantInfo.ServiceProvider]);
                ilGen.Emit(OpCodes.Ret);
            }
            else
            {
                foreach (var constructor in constructorInfos)
                {
                    Type[] parameterTypes = constructor.GetParameters().Select(i => i.ParameterType).Concat(new Type[] { typeof(IServiceProvider) }).ToArray();
                    var constructorBuilder = context.TypeBuilder.DefineConstructor(context.ServiceType.IsAbstract ? constructor.Attributes | MethodAttributes.Public : constructor.Attributes, constructor.CallingConvention, parameterTypes);
                    foreach (var customAttributeData in constructor.CustomAttributes)
                    {
                        constructorBuilder.SetCustomAttribute(DefineCustomAttribute(customAttributeData));
                    }
                    DefineParameters(constructor, constructorBuilder);

                    var ilGen = constructorBuilder.GetILGenerator();

                    ilGen.EmitThis();
                    for (var i = 1; i < parameterTypes.Length; i++)
                    {
                        ilGen.EmitLoadArg(i);
                    }
                    ilGen.Emit(OpCodes.Call, constructor);
                    ilGen.EmitThis();
                    ilGen.EmitLoadArg(parameterTypes.Length);
                    ilGen.Emit(OpCodes.Stfld, context.Fields[ConstantInfo.ServiceProvider]);
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