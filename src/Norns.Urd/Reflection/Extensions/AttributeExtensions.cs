using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.Reflection
{
    public static class AttributeExtensions
    {
        public static void DefineParameters(this ConstructorBuilder constructorBuilder, ConstructorInfo constructor)
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

        public static CustomAttributeBuilder DefineCustomAttribute(this CustomAttributeData customAttributeData)
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

        public static CustomAttributeBuilder DefineCustomAttribute(Type attributeType)
        {
            return new CustomAttributeBuilder(attributeType.GetConstructor(Type.EmptyTypes), Array.Empty<object>());
        }

        public static CustomAttributeBuilder DefineCustomAttribute<T>() where T : Attribute
        {
            return DefineCustomAttribute(typeof(T));
        }

        public static CustomAttributeReflector[] GetCustomAttributeReflectors(this ICustomAttributeReflectorProvider provider)
        {
            return provider.CustomAttributeReflectors;
        }

        public static IEnumerable<CustomAttributeReflector> GetCustomAttributeReflectors(this ICustomAttributeReflectorProvider provider, Type attributeType)
        {
            var reflectors = provider.GetCustomAttributeReflectors();
            if (reflectors.Length == 0)
            {
                return reflectors;
            }
            else
            {
                var attrToken = attributeType.TypeHandle;
                return reflectors.Where(i => i.Tokens.Contains(attrToken));
            }
        }

        public static IEnumerable<Attribute> GetCustomAttributes(this ICustomAttributeReflectorProvider provider, Type attributeType)
        {
            return provider.GetCustomAttributeReflectors(attributeType).Select(i => i.Invoke());
        }

        public static IEnumerable<T> GetCustomAttributes<T>(this ICustomAttributeReflectorProvider provider) where T : Attribute
        {
            return provider.GetCustomAttributes(typeof(T)).Select(i => i as T);
        }

        public static Attribute GetCustomAttribute(this ICustomAttributeReflectorProvider provider, Type attributeType)
        {
            return provider.GetCustomAttributeReflectors(attributeType).Select(i => i.Invoke()).FirstOrDefault();
        }

        public static T GetCustomAttribute<T>(this ICustomAttributeReflectorProvider provider) where T : Attribute
        {
            return provider.GetCustomAttribute(typeof(T)) as T;
        }

        public static bool IsDefined(this ICustomAttributeReflectorProvider provider, Type attributeType)
        {
            return provider.GetCustomAttributeReflectors(attributeType).FirstOrDefault() != null;
        }

        public static bool IsDefined<T>(this ICustomAttributeReflectorProvider provider) where T : Attribute
        {
            return provider.IsDefined(typeof(T));
        }
    }
}