﻿using Norns.Urd.Attributes;
using Norns.Urd.Interceptors;
using Norns.Urd.Reflection;
using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.DynamicProxy
{
    public abstract class ProxyBuilderBase : IProxyBuilder
    {
        protected abstract ProxyTypes ProxyType { get; }

        public Type Create(Type serviceType, IInterceptorConfiguration configuration, ModuleBuilder moduleBuilder)
        {
            if (IsIgnoreType(serviceType, configuration)) return null;
            var context = new ProxyGeneratorContext(moduleBuilder, serviceType, configuration, ProxyType);
            DefineFields(context);
            DefineCustomAttributes(context);
            DefineConstructors(context);
            return context.Complete();
        }

        #region Constructor

        private void DefineConstructors(in ProxyGeneratorContext context)
        {
            var constructorInfos = context.ServiceType.DeclaredConstructors
                .Where(c => !c.IsStatic && c.IsVisible())
                .ToArray();
            if (constructorInfos.Length == 0)
            {
                DefineDefaultConstructor(context);
            }
            else
            {
                foreach (var constructor in constructorInfos)
                {
                    DefineConstructor(context, constructor);
                }
            }
        }

        private static void DefineConstructor(in ProxyGeneratorContext context, ConstructorInfo constructor)
        {
            Type[] parameterTypes = constructor.GetParameters().Select(i => i.ParameterType).Concat(Constants.DefaultConstructorParameters).ToArray();
            var constructorBuilder = context.ProxyType.TypeBuilder.DefineConstructor(context.ServiceType.IsAbstract ? constructor.Attributes | MethodAttributes.Public : constructor.Attributes, constructor.CallingConvention, parameterTypes);
            foreach (var customAttributeData in constructor.CustomAttributes)
            {
                constructorBuilder.SetCustomAttribute(customAttributeData.DefineCustomAttribute());
            }
            constructorBuilder.DefineParameters(constructor);

            var il = constructorBuilder.GetILGenerator();

            il.EmitThis();
            for (var i = 1; i < parameterTypes.Length; i++)
            {
                il.EmitLoadArg(i);
            }
            il.Emit(OpCodes.Call, constructor);
            il.EmitThis();
            il.EmitLoadArg(parameterTypes.Length);
            il.Emit(OpCodes.Stfld, context.ProxyType.Fields[Constants.ServiceProvider]);
            il.Emit(OpCodes.Ret);
        }

        private static void DefineDefaultConstructor(in ProxyGeneratorContext context)
        {
            var constructorBuilder = context.ProxyType.TypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Constants.DefaultConstructorParameters);

            var il = constructorBuilder.GetILGenerator();
            il.EmitThis();
            il.Emit(OpCodes.Call, Constants.ObjectCtor);
            il.EmitThis();
            il.EmitLoadArg(1);
            il.Emit(OpCodes.Stfld, context.ProxyType.Fields[Constants.ServiceProvider]);
            il.Emit(OpCodes.Ret);
        }

        #endregion Constructor

        private bool IsIgnoreType(Type serviceType, IInterceptorConfiguration configuration) => serviceType switch
        {
            { IsSealed: true }
            or { IsValueType: true }
            or { IsEnum: true }
                    => true,
            _ when !serviceType.GetTypeInfo().IsVisible() || serviceType.GetReflector().IsDefined<NonAspectAttribute>() => true,
            _ => configuration.IsIgnoreType(serviceType)
        };

        private void DefineCustomAttributes(in ProxyGeneratorContext context)
        {
            context.ProxyType.TypeBuilder.SetCustomAttribute(AttributeExtensions.DefineCustomAttribute<NonAspectAttribute>());
            context.ProxyType.TypeBuilder.SetCustomAttribute(AttributeExtensions.DefineCustomAttribute<DynamicProxyAttribute>());
        }

        public virtual void DefineFields(in ProxyGeneratorContext context)
        {
            context.ProxyType.Fields.Add(Constants.ServiceProvider, context.ProxyType.TypeBuilder.DefineField(Constants.ServiceProvider, typeof(IServiceProvider), FieldAttributes.Private));
        }
    }

    public class FacadeProxyBuilder : ProxyBuilderBase
    {
        protected override ProxyTypes ProxyType { get; } = ProxyTypes.Facade;

        public override void DefineFields(in ProxyGeneratorContext context)
        {
            base.DefineFields(context);
            context.ProxyType.Fields.Add(Constants.Instance, context.ProxyType.TypeBuilder.DefineField(Constants.Instance, context.ServiceType, FieldAttributes.Private));
        }
    }

    public class InheritProxyBuilder : ProxyBuilderBase
    {
        protected override ProxyTypes ProxyType { get; } = ProxyTypes.Inherit;
    }
}