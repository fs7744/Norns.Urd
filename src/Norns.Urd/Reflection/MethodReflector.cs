using System.Linq;
using System.Reflection;

namespace Norns.Urd.Reflection
{
    public class MethodReflector : MemberReflector<MethodInfo>
    {
        public PropertyInfo BindingProperty { get; }

        public MethodReflector(MethodInfo methodInfo) : base(methodInfo)
        {
            BindingProperty = GetBindingProperty(methodInfo);
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
    }
}