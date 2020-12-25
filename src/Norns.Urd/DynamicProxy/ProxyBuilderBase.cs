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
        }

        private Type CreateInternal(Type serviceType, IInterceptorCreator interceptorCreator, ModuleBuilder moduleBuilder)
        {
            if (IsNonAspectType(serviceType, interceptorCreator)) return null;
            var context = new ProxyGeneratorContext(moduleBuilder, serviceType, interceptorCreator, ProxyType);
            DefineFields(context);
            DefineCustomAttributes(context);
            DefineConstructors(context);
            DefineMethods(context);
            DefineProperties(context);
            return context.Complete();
        }

        protected void DefineProperties(in ProxyGeneratorContext context)
        {
            var (il, _) = context.ProxyType.PropertyInject;
            GetServiceInstance(context, il);
            il.EmitLoadArg(1);
            il.Emit(OpCodes.Call, Constants.PropertyInject);

            var properties = context.ServiceType.GetTypeInfo().GetProperties(Constants.MethodBindingFlags);
            foreach (var property in context.ServiceType.GetTypeInfo().GetProperties(Constants.MethodBindingFlags))
            {
                DefineProperty(context, property);
            }

            var dicts = properties.ToDictionary(i => i.Name, i => i);
            foreach (var item in context.ServiceType.ImplementedInterfaces)
            {
                foreach (var property in item.GetTypeInfo().DeclaredProperties)
                {
                    DefineImplementedInterfaceProperty(context, property, dicts.TryGetValue(property.Name, out var p) ? p : property);
                }
            }
        }

        public virtual PropertyBuilder DefineImplementedInterfaceProperty(in ProxyGeneratorContext context, PropertyInfo property, PropertyInfo implementedProperty)
        {
            PropertyBuilder propertyBuilder = null;
            var getMethod = property.CanRead
                ? DefineMethod(context, property.GetMethod, implementedProperty.GetMethod)
                : null;

            var setMethod = property.CanWrite
                ? DefineMethod(context, property.SetMethod, implementedProperty.SetMethod)
                : null;

            if (getMethod != null || setMethod != null)
            {
                propertyBuilder = context.ProxyType.TypeBuilder.DefineProperty($"{property.DeclaringType.GetReflector().FullDisplayName}.{property.Name}", property.Attributes, property.PropertyType, Type.EmptyTypes);
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
            return propertyBuilder;
        }

        public virtual PropertyBuilder DefineProperty(in ProxyGeneratorContext context, PropertyInfo property)
        {
            PropertyBuilder propertyBuilder = null;
            var getMethod = property.CanRead && property.GetMethod.IsVisibleAndVirtual()
                ? DefineMethod(context, property.GetMethod, property.GetMethod)
                : null;

            var setMethod = property.CanWrite && property.SetMethod.IsVisibleAndVirtual()
                ? DefineMethod(context, property.SetMethod, property.SetMethod)
                : null;

            if (getMethod != null || setMethod != null)
            {
                propertyBuilder = context.ProxyType.TypeBuilder.DefineProperty(property.Name, property.Attributes, property.PropertyType, Type.EmptyTypes);
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
            return propertyBuilder;
        }

        private void DefineMethods(in ProxyGeneratorContext context)
        {
            var methods = context.ServiceType.GetMethods(Constants.MethodBindingFlags);
            foreach (var method in methods
                .Where(x => x.IsNotPropertyBinding()
                    && !Constants.IgnoreMethods.Contains(x.Name)
                    && x.IsVisibleAndVirtual()))
            {
                DefineMethod(context, method, method);
            }
            var dicts = methods.ToDictionary(i => i.GetReflector().DisplayName, i => i);
            foreach (var method in context.ServiceType.ImplementedInterfaces
                .SelectMany(i => i.GetTypeInfo().GetMethods(Constants.MethodBindingFlags))
                .Where(x => x.IsNotPropertyBinding()))
            {
                DefineMethod(context, method, dicts.TryGetValue(method.GetReflector().DisplayName, out var im) ? im : method);
            }
        }

        private MethodBuilder DefineMethod(in ProxyGeneratorContext context, MethodInfo method, MethodInfo implementationMethod)
        {
            if (context.InterceptorCreator.IsNonAspectMethod(method))
            {
                return DefineNonAspectMethod(context, method);
            }
            else
            {
                return DefineProxyMethod(context, method, implementationMethod);
            }
        }

        protected abstract MethodBuilder DefineNonAspectMethod(in ProxyGeneratorContext context, MethodInfo method);

        protected abstract void GetServiceInstance(in ProxyGeneratorContext context, ILGenerator il);

        protected abstract FieldBuilder DefineMethodInfoCaller(in ProxyGeneratorContext context, MethodInfo method);

        protected abstract void CallPropertyInjectInConstructor(in ProxyGeneratorContext context, ILGenerator il);

        protected MethodBuilder DefineProxyMethod(in ProxyGeneratorContext context, MethodInfo method, MethodInfo implementationMethod)
        {
            var p = method.GetParameters();
            var parameters = p.Select(i => i.ParameterType).ToArray();
            MethodBuilder methodBuilder = context.ProxyType.TypeBuilder.DefineMethod($"{method.DeclaringType.GetReflector().FullDisplayName}.{method.Name}",
                DefineProxyMethodAttributes(method), method.CallingConvention, method.ReturnType, parameters);
            methodBuilder.DefineGenericParameter(method);
            methodBuilder.DefineParameters(method);
            methodBuilder.DefineCustomAttributes(method);
            var il = methodBuilder.GetILGenerator();
            var caller = DefineMethodInfoCaller(context, implementationMethod);
            if (method.ContainsGenericParameters && !method.IsGenericMethodDefinition)
            {
                il.Emit(OpCodes.Ldsfld, caller);
                il.EmitMethod(method);
            }
            else if (method.IsGenericMethodDefinition)
            {
                il.Emit(OpCodes.Ldsfld, caller);
                var gm = method.MakeGenericMethod(methodBuilder.GetGenericArguments());
                il.EmitMethod(gm);
            }
            else
            {
                il.Emit(OpCodes.Ldsfld, caller);
                il.Emit(OpCodes.Ldsfld, context.AssistType.DefineMethodInfoCache(method));
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
                    il.Emit(OpCodes.Ldloc, c);
                    il.Emit(OpCodes.Call, Constants.AwaitResultTask);
                    il.Emit(OpCodes.Call, Constants.GetObjectTaskResult);
                }
                else
                {
                    il.Emit(OpCodes.Ldloc, c);
                    il.Emit(OpCodes.Call, Constants.GetReturnValue);
                }
                    il.EmitConvertObjectTo(method.ReturnType);
            }
            for (var i = 0; i < parameters.Length; i++)
            {
                if (parameters[i].IsByRef && !p[i].IsReadOnly())
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

        private static MethodAttributes DefineProxyMethodAttributes(MethodInfo method)
        {
            if (method.DeclaringType.IsInterface)
            {
                return MethodAttributes.Public | MethodAttributes.Final | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual;
            }
            var attributes = MethodAttributes.HideBySig | MethodAttributes.Virtual;

            if (method.Attributes.HasFlag(MethodAttributes.Public))
            {
                attributes |= MethodAttributes.Public;
            }

            if (method.Attributes.HasFlag(MethodAttributes.Family))
            {
                attributes |= MethodAttributes.Family;
            }

            if (method.Attributes.HasFlag(MethodAttributes.FamORAssem))
            {
                attributes |= MethodAttributes.FamORAssem;
            }

            return attributes;
        }

        #region Constructor

        protected virtual void DefineConstructors(in ProxyGeneratorContext context)
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

        protected ParameterInfo[] DefineConstructor(in ProxyGeneratorContext context, ConstructorInfo constructor)
        {
            var oldParameters = constructor.GetParameters();
            Type[] parameterTypes = oldParameters.Select(i => i.ParameterType).Concat(Constants.DefaultConstructorParameters).ToArray();
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
            CallPropertyInjectInConstructor(context, il);
            il.Emit(OpCodes.Ret);
            return oldParameters;
        }

        protected void DefineDefaultConstructor(in ProxyGeneratorContext context)
        {
            var constructorBuilder = context.ProxyType.TypeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, Constants.DefaultConstructorParameters);
            constructorBuilder.SetCustomAttribute(AttributeExtensions.DefineCustomAttribute<DynamicProxyAttribute>());
            var il = constructorBuilder.GetILGenerator();
            il.EmitThis();
            il.Emit(OpCodes.Call, Constants.ObjectCtor);
            il.EmitThis();
            il.EmitLoadArg(1);
            il.Emit(OpCodes.Stfld, context.ProxyType.Fields[Constants.ServiceProvider]);
            CallPropertyInjectInConstructor(context, il);
            il.Emit(OpCodes.Ret);
        }

        #endregion Constructor

        private bool IsNonAspectType(Type serviceType, IInterceptorCreator interceptorCreator)
        {
            return serviceType.IsSealed || serviceType.IsValueType || serviceType.IsEnum || interceptorCreator.IsNonAspectType(serviceType);
        }

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