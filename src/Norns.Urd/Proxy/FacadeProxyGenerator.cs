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
        protected const string InterceptorFactory = "interceptorFactory";
        protected static readonly ConstructorInfo ObjectCtor = typeof(object).GetTypeInfo().DeclaredConstructors.Single();
        protected readonly IInterceptorFactory interceptorFactory;
        private const MethodAttributes OverrideMethodAttributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;
        public virtual ProxyTypes ProxyType { get; } = ProxyTypes.Facade;

        public FacadeProxyGenerator(IInterceptorFactory interceptorFactory)
        {
            this.interceptorFactory = interceptorFactory;
        }

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
            //var dp = implTypeBuilder.DefineProperty(p.Name, p.Attributes, p.PropertyType, Type.EmptyTypes);
            //if (p.CanRead)
            //{
            //    var method = MethodBuilderUtils.DefineClassMethod(p.GetMethod, implType, typeDesc);
            //    dp.SetGetMethod(method);
            //}
            //if (p.CanWrite)
            //{
            //    var method = MethodBuilderUtils.DefineClassMethod(p.SetMethod, implType, typeDesc);
            //    dp.SetSetMethod(method);
            //}
            return context.TypeBuilder.CreateTypeInfo().AsType();
        }

        public virtual void DefineMethods(ProxyGeneratorContext context)
        {
            foreach (var method in context.ServiceType.GetTypeInfo().DeclaredMethods)
            {
                MethodBuilder methodBuilder = context.TypeBuilder.DefineMethod(method.Name, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, method.GetParameters().Select(i => i.ParameterType).ToArray());
                var ilGen = methodBuilder.GetILGenerator();
                ilGen.Emit(OpCodes.Ret);
                //context.TypeBuilder.DefineMethodOverride(methodBuilder, method);
                interceptorFactory.CreateInterceptor(method, c => c.ReturnValue = method.Invoke(c.Service, c.Parameters), ProxyTypes.Facade);
            } 
        }

        public virtual void DefineType(ProxyGeneratorContext context)
        {
            var serviceType = context.ServiceType;
            var (parent, interfaceTypes) = serviceType.IsInterface
                    ? (typeof(object), new Type[] { serviceType })
                    : (serviceType, Type.EmptyTypes);
            context.TypeBuilder = context.ModuleBuilder.DefineType(context.ProxyTypeName, TypeAttributes.Class | TypeAttributes.Public | TypeAttributes.Sealed, parent, interfaceTypes);
        }

        public virtual void DefineFields(ProxyGeneratorContext context)
        {
            context.Fields.Add(Instance, context.TypeBuilder.DefineField(Instance, context.ServiceType, FieldAttributes.Private));
            context.Fields.Add(InterceptorFactory, context.TypeBuilder.DefineField(InterceptorFactory, typeof(IInterceptorFactory), FieldAttributes.Private));
        }

        public virtual void DefineConstructors(ProxyGeneratorContext context)
        {
            var constructorInfos = context.ServiceType.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic && (c.IsPublic || c.IsFamily || c.IsFamilyAndAssembly || c.IsFamilyOrAssembly)).ToArray();
            if (constructorInfos.Length == 0)
            {
                var constructorBuilder = context.TypeBuilder.DefineConstructor(MethodAttributes.Public, ObjectCtor.CallingConvention, new Type[] { typeof(IInterceptorFactory) });

                constructorBuilder.DefineParameter(1, ParameterAttributes.None, InterceptorFactory);

                var ilGen = constructorBuilder.GetILGenerator();
                ilGen.EmitThis();
                ilGen.Emit(OpCodes.Call, ObjectCtor);

                ilGen.EmitThis();
                ilGen.EmitLoadArg(1);
                ilGen.Emit(OpCodes.Stfld, context.Fields[InterceptorFactory]);

                ilGen.Emit(OpCodes.Ret);
            }
            else
            {
                foreach (var constructor in constructorInfos)
                {
                    Type[] parameterTypes = new Type[] { typeof(IInterceptorFactory) }.Concat(constructor.GetParameters().Select(i => i.ParameterType)).ToArray();
                    var constructorBuilder = context.TypeBuilder.DefineConstructor(context.ServiceType.IsAbstract ? constructor.Attributes | MethodAttributes.Public : constructor.Attributes, constructor.CallingConvention, parameterTypes);
                    foreach (var customAttributeData in constructor.CustomAttributes)
                    {
                        constructorBuilder.SetCustomAttribute(DefineCustomAttribute(customAttributeData));
                    }
                    DefineParameters(constructor, constructorBuilder);

                    var ilGen = constructorBuilder.GetILGenerator();

                    ilGen.EmitThis();
                    ilGen.EmitLoadArg(1);
                    ilGen.Emit(OpCodes.Stfld, context.Fields[InterceptorFactory]);

                    ilGen.EmitThis();
                    for (var i = 2; i <= parameterTypes.Length; i++)
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
            constructorBuilder.DefineParameter(1, ParameterAttributes.None, InterceptorFactory);
            var parameters = constructor.GetParameters();
            if (parameters.Length > 0)
            {
                var paramOffset = 2;
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

        //internal static MethodBuilder DefineClassMethod(MethodInfo method, Type implType, TypeDesc typeDesc)
        //{
        //    var attributes = OverrideMethodAttributes;

        //    if (method.Attributes.HasFlag(MethodAttributes.Public))
        //    {
        //        attributes = attributes | MethodAttributes.Public;
        //    }

        //    if (method.Attributes.HasFlag(MethodAttributes.Family))
        //    {
        //        attributes = attributes | MethodAttributes.Family;
        //    }

        //    if (method.Attributes.HasFlag(MethodAttributes.FamORAssem))
        //    {
        //        attributes = attributes | MethodAttributes.FamORAssem;
        //    }

        //    var methodBuilder = DefineMethod(method, method.Name, attributes, implType, typeDesc);
        //    return methodBuilder;
        //}
    }
}