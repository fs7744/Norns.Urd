using System.Reflection;

namespace Norns.Urd.Reflection
{
    public class PropertyReflector : MemberReflector<PropertyInfo>
    {
        public PropertyReflector(PropertyInfo memberInfo) : base(memberInfo)
        {
        }
    }
}