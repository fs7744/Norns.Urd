using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Norns.Urd.Reflection
{
    public static class MethodExtensions
    {
        public static MethodReflector GetReflector(this MethodInfo method)
        {
            static MethodReflector Create(MethodInfo t) => new MethodReflector(t);
            return ReflectorCache<MethodInfo, MethodReflector>.GetOrAdd(method, Create);
        }

        public static bool IsNotPropertyBinding(this MethodInfo method) => method.GetReflector().BindingProperty == null;

        public static bool IsVisibleAndVirtual(this MethodInfo method)
        {
            if (method.IsStatic || method.IsFinal)
            {
                return false;
            }
            return method.IsVirtual &&
                    (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);
        }

        public static bool IsVoid(this MethodInfo methodInfo) => methodInfo.ReturnType == typeof(void);

        public static void DefineGenericParameter(this MethodBuilder methodBuilder, MethodInfo tergetMethod)
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

        public static void DefineCustomAttributes(this MethodBuilder methodBuilder, MethodInfo method)
        {
            foreach (var customAttributeData in method.CustomAttributes)
            {
                methodBuilder.SetCustomAttribute(customAttributeData.DefineCustomAttribute());
            }
        }
    }
}