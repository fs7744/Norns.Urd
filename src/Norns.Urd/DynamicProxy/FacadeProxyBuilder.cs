using Norns.Urd.Attributes;
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
            DefineMethods(context);
            return context.Complete();
        }

        private void DefineMethods(in ProxyGeneratorContext context)
        {
            foreach (var method in context.ServiceType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.IsNotPropertyBinding()
                    && !Constants.IgnoreMethods.Contains(x.Name)
                    && x.IsVisibleAndVirtual()))
            {
                if (method.GetReflector().IsDefined<NonAspectAttribute>())
                {
                    DefineNonAspectMethod(context, method);
                }
                else
                {
                    DefineMethod(context, method);
                }
            }
        }

        protected abstract void DefineNonAspectMethod(in ProxyGeneratorContext context, MethodInfo method);

        protected virtual void DefineMethod(in ProxyGeneratorContext context, MethodInfo method)
        {
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

        protected override void DefineNonAspectMethod(in ProxyGeneratorContext context, MethodInfo method)
        {
            var parameters = method.GetParameters().Select(i => i.ParameterType).ToArray();
            MethodBuilder methodBuilder = context.ProxyType.TypeBuilder.DefineMethod(method.Name, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, parameters);
            methodBuilder.DefineCustomAttributes(method);
            methodBuilder.DefineGenericParameter(method);
            var il = methodBuilder.GetILGenerator();
            il.EmitThis();
            il.Emit(OpCodes.Ldfld, context.ProxyType.Fields[Constants.Instance]);
            for (var i = 1; i <= parameters.Length; i++)
            {
                il.EmitLoadArg(i);
            }
            il.Emit(OpCodes.Callvirt, method);
            il.Emit(OpCodes.Ret);
            context.ProxyType.TypeBuilder.DefineMethodOverride(methodBuilder, method);
        }
    }

    public class InheritProxyBuilder : ProxyBuilderBase
    {
        protected override ProxyTypes ProxyType { get; } = ProxyTypes.Inherit;

        protected override void DefineNonAspectMethod(in ProxyGeneratorContext context, MethodInfo method)
        {
            var parameters = method.GetParameters().Select(i => i.ParameterType).ToArray();
            MethodBuilder methodBuilder = context.ProxyType.TypeBuilder.DefineMethod(method.Name, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, parameters);
            methodBuilder.DefineCustomAttributes(method);
            methodBuilder.DefineGenericParameter(method);
            var il = methodBuilder.GetILGenerator();
            if (method.IsAbstract)
            {
                il.EmitDefault(method.ReturnType);
            }
            else
            {
                il.EmitThis();
                for (var i = 1; i <= parameters.Length; i++)
                {
                    il.EmitLoadArg(i);
                }
                il.Emit(OpCodes.Call, method);
            }

            il.Emit(OpCodes.Ret);
            context.ProxyType.TypeBuilder.DefineMethodOverride(methodBuilder, method);
        }
    }
}