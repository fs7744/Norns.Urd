using System.Linq;
using System.Reflection;

namespace Norns.Urd.Reflection
{
    public class ParameterReflector : ICustomAttributeReflectorProvider
    {
        public ParameterInfo MemberInfo { get; }
        public CustomAttributeReflector[] CustomAttributeReflectors { get; }

        public ParameterReflector(ParameterInfo parameter)
        {
            MemberInfo = parameter;
            CustomAttributeReflectors = parameter.CustomAttributes.Select(data => CustomAttributeReflector.Create(data)).ToArray();
        }
    }
}