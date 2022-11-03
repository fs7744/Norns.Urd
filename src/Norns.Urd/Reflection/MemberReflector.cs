using System.Linq;
using System.Reflection;

namespace Norns.Urd.Reflection
{
    public interface ICustomAttributeReflectorProvider
    {
        CustomAttributeReflector[] CustomAttributeReflectors { get; }
    }

    public class MemberReflector<TMemberInfo> : ICustomAttributeReflectorProvider where TMemberInfo : MemberInfo
    {
        public TMemberInfo MemberInfo { get; }
        public CustomAttributeReflector[] CustomAttributeReflectors { get; }

        public MemberReflector(TMemberInfo memberInfo)
        {
            MemberInfo = memberInfo;
            CustomAttributeReflectors = memberInfo.CustomAttributes.Select(data => CustomAttributeReflector.Create(data)).ToArray();
        }
    }
}