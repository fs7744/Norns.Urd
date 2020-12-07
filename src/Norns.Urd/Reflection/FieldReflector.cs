using System.Reflection;

namespace Norns.Urd.Reflection
{
    public class FieldReflector : MemberReflector<FieldInfo>
    {
        public FieldReflector(FieldInfo memberInfo) : base(memberInfo)
        {
        }
    }
}