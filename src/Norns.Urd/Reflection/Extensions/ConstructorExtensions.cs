using System.Reflection;

namespace Norns.Urd.Reflection
{
    public static class ConstructorExtensions
    {
        public static bool IsVisible(this ConstructorInfo constructorInfo)
        {
            return constructorInfo.IsPublic
                || constructorInfo.IsFamily
                || constructorInfo.IsFamilyAndAssembly
                || constructorInfo.IsFamilyOrAssembly;
        }
    }
}