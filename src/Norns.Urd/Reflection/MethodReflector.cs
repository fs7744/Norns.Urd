using System.Linq;
using System.Reflection;
using System.Text;

namespace Norns.Urd.Reflection
{
    public class MethodReflector : MemberReflector<MethodInfo>
    {
        public string DisplayName { get; }
        public PropertyInfo BindingProperty { get; }

        public MethodReflector(MethodInfo methodInfo) : base(methodInfo)
        {
            BindingProperty = GetBindingProperty(methodInfo);
            DisplayName = GetDisplayName(methodInfo);
        }

        private static PropertyInfo GetBindingProperty(MethodInfo method)
        {
            static MethodInfo GetPropertyMethod(PropertyInfo p) => p switch
            {
                { CanRead: true } => p.GetMethod,
                _ => p.SetMethod
            };
            return method.DeclaringType.GetTypeInfo()
                .GetProperties()
                .FirstOrDefault(i => GetPropertyMethod(i) == method);
        }

        private static string GetDisplayName(MethodInfo method)
        {
            var name = new StringBuilder(method.ReturnType.GetReflector().DisplayName)
                .Append(' ')
                .Append(method.Name);
            if (method.IsGenericMethod)
            {
                name.Append('<');
                var arguments = method.GetGenericArguments();
                name.Append(arguments[0].GetReflector().DisplayName);
                for (var i = 1; i < arguments.Length; i++)
                {
                    name.Append(',');
                    name.Append(arguments[i].GetReflector().DisplayName);
                }
                name.Append('>');
            }
            var parameterTypes = method.GetParameters().Select(i => i.ParameterType).ToArray();
            name.Append('(');
            if (parameterTypes.Length == 0)
            {
                name.Append(')');
                return name.ToString();
            }
            name.Append(parameterTypes[0].GetReflector().DisplayName);
            for (var i = 1; i < parameterTypes.Length; i++)
            {
                name.Append(',');
                name.Append(parameterTypes[i].GetReflector().DisplayName);
            }
            name.Append(')');
            return name.ToString();
        }
    }
}