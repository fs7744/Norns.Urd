using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Norns.Urd.Reflection
{
    public class MethodReflector : MemberReflector<MethodInfo>
    {
        public string DisplayName { get; }
        public PropertyInfo BindingProperty { get; }
        public ParameterReflector[] Parameters { get; }

        public int CancellationTokenIndex { get; }

        public MethodReflector(MethodInfo methodInfo) : base(methodInfo)
        {
            BindingProperty = GetBindingProperty(methodInfo);
            DisplayName = GetDisplayName(methodInfo);
            var cancellationTokenIndex = -1;
            Parameters = methodInfo.GetParameters().Select(i =>
            {
                if (i.ParameterType == typeof(CancellationToken))
                {
                    cancellationTokenIndex = i.Position;
                }
                return new ParameterReflector(i);
            }).ToArray();
            CancellationTokenIndex = cancellationTokenIndex;
        }

        private static PropertyInfo GetBindingProperty(MethodInfo method)
        {
            foreach (var item in method.DeclaringType.GetTypeInfo().GetProperties())
            {
                if (item.CanRead && item.GetMethod == method) return item;
                if (item.CanWrite && item.SetMethod == method) return item;
            }
            return null;
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