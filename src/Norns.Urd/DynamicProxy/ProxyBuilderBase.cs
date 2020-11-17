using Norns.Urd.Attributes;
using Norns.Urd.Interceptors;
using Norns.Urd.Reflection;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.DynamicProxy
{
    public abstract class ProxyBuilderBase : IProxyBuilder
    {
        protected readonly ConcurrentDictionary<Type, Type> cache = new ConcurrentDictionary<Type, Type>();
        protected abstract ProxyTypes ProxyType { get; }

        public Type Create(Type serviceType, IInterceptorCreator interceptorCreator, ModuleBuilder moduleBuilder)
        {
            return cache.GetOrAdd(serviceType, t => CreateInternal(t, interceptorCreator, moduleBuilder));

            Type CreateInternal(Type serviceType, IInterceptorCreator interceptorCreator, ModuleBuilder moduleBuilder)
            {
                if (IsIgnoreType(serviceType, interceptorCreator)) return null;
                var context = new ProxyGeneratorContext(moduleBuilder, serviceType, interceptorCreator, ProxyType);
                DefineFields(context);
                DefineCustomAttributes(context);
                DefineConstructors(context);
                DefineMethods(context);
                DefineProperties(context);
                return context.Complete();
            }
        }

        protected void DefineProperties(in ProxyGeneratorContext context)
        {
            foreach (var property in context.ServiceType.GetTypeInfo().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                DefineProperty(context, property);
            }
        }

        public virtual void DefineProperty(ProxyGeneratorContext context, PropertyInfo property)
        {
            var getMethod = property.CanRead && property.GetMethod.IsVisibleAndVirtual()
                ? DefineMethod(context, property.GetMethod)
                : null;

            var setMethod = property.CanWrite && property.SetMethod.IsVisibleAndVirtual()
                ? DefineMethod(context, property.SetMethod)
                : null;

            if (getMethod != null || setMethod != null)
            {
                var propertyBuilder = context.AssistType.TypeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, Type.EmptyTypes);
                if (getMethod != null)
                {
                    propertyBuilder.SetGetMethod(getMethod);
                }
                if (setMethod != null)
                {
                    propertyBuilder.SetSetMethod(setMethod);
                }
                foreach (var customAttributeData in property.CustomAttributes)
                {
                    propertyBuilder.SetCustomAttribute(customAttributeData.DefineCustomAttribute());
                }
            }
        }

        private void DefineMethods(in ProxyGeneratorContext context)
        {
            foreach (var method in context.ServiceType
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => x.IsNotPropertyBinding()
                    && !Constants.IgnoreMethods.Contains(x.Name)
                    && x.IsVisibleAndVirtual()))
            {
                DefineMethod(context, method);
            }
        }

        private MethodBuilder DefineMethod(in ProxyGeneratorContext context, MethodInfo method)
        {
            if (method.GetReflector().IsDefined<NonAspectAttribute>())
            {
                return DefineNonAspectMethod(context, method);
            }
            else
            {
                return DefineProxyMethod(context, method);
            }
        }

        protected abstract MethodBuilder DefineNonAspectMethod(in ProxyGeneratorContext context, MethodInfo method);

        protected abstract void GetServiceInstance(in ProxyGeneratorContext context, ILGenerator il);

        protected abstract FieldBuilder DefineMethodInfoCaller(in ProxyGeneratorContext context, MethodInfo method);

        protected MethodBuilder DefineProxyMethod(in ProxyGeneratorContext context, MethodInfo method)
        {
            var parameters = method.GetParameters().Select(i => i.ParameterType).ToArray();
            MethodBuilder methodBuilder = context.ProxyType.TypeBuilder.DefineMethod(method.Name, MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, method.CallingConvention, method.ReturnType, parameters);
            methodBuilder.DefineCustomAttributes(method);
            methodBuilder.DefineGenericParameter(method);
            var il = methodBuilder.GetILGenerator();
            var caller = DefineMethodInfoCaller(context, method);
            if (method.ContainsGenericParameters && !method.IsGenericMethodDefinition)
            {
                il.Emit(OpCodes.Ldsfld, caller);
                il.Emit(OpCodes.Ldtoken, method);
                il.Emit(OpCodes.Ldtoken, method.DeclaringType);
                il.Emit(OpCodes.Call, Constants.GetMethodFromHandle);
                il.Emit(OpCodes.Castclass, typeof(MethodInfo));
            }
            else
            {
                if (method.IsGenericMethodDefinition)
                {
                    il.Emit(OpCodes.Ldsfld, caller);
                    var gm = method.MakeGenericMethod(methodBuilder.GetGenericArguments());
                    il.Emit(OpCodes.Ldtoken, gm);
                    il.Emit(OpCodes.Ldtoken, gm.DeclaringType);
                    il.Emit(OpCodes.Call, Constants.GetMethodFromHandle);
                    il.Emit(OpCodes.Castclass, typeof(MethodInfo));
                }
                else
                {
                    il.Emit(OpCodes.Ldsfld, caller);
                    il.Emit(OpCodes.Ldsfld, context.AssistType.DefineMethodInfoCache(method));
                }
            }
            GetServiceInstance(context, il);

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
            il.Emit(OpCodes.Ldfld, context.ProxyType.Fields[Constants.ServiceProvider]);
            il.Emit(OpCodes.Newobj, Constants.AspectContextCtor);
            var call = method.IsAsync() ? Constants.InvokeAsync : Constants.Invoke;
            if (method.IsVoid() || method.IsTask() || method.IsValueTask())
            {
                il.Emit(OpCodes.Call, call);
            }
            else
            {
                var c = il.DeclareLocal(typeof(AspectContext));
                il.Emit(OpCodes.Stloc, c);
                il.Emit(OpCodes.Ldloc, c);
                il.Emit(OpCodes.Call, call);
                if (method.IsReturnTask() || method.IsReturnValueTask())
                {
                    il.Emit(OpCodes.Pop);
                }
                il.Emit(OpCodes.Ldloc, c);
                il.Emit(OpCodes.Call, Constants.GetReturnValue);
                il.EmitConvertObjectTo(method.ReturnType);
            }
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].IsByRef)
                {
                    il.EmitLoadArg(i + 1);
                    il.Emit(OpCodes.Ldloc, argsLocal);
                    il.EmitInt(i);
                    il.Emit(OpCodes.Ldelem_Ref);
                    il.EmitConvertObjectTo(parameters[i].GetElementType());
                    il.EmitStRef(parameters[i]);
                }
            }
            il.Emit(OpCodes.Ret);
            context.ProxyType.TypeBuilder.DefineMethodOverride(methodBuilder, method);
            return methodBuilder;
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

        private bool IsIgnoreType(Type serviceType, IInterceptorCreator interceptorCreator) => serviceType switch
        {
            { IsSealed: true }
            or { IsValueType: true }
            or { IsEnum: true }
                    => true,
            _ when !serviceType.GetTypeInfo().IsVisible() || serviceType.GetReflector().IsDefined<NonAspectAttribute>() => true,
            _ => interceptorCreator.IsIgnoreType(serviceType)
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
}