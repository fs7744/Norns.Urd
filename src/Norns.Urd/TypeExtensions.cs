using System;
using System.Reflection;

namespace Norns.Urd
{
    public static class TypeExtensions
    {
        public static bool IsVisible(this TypeInfo typeInfo)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }
            if (typeInfo.IsNested)
            {
                if (!typeInfo.DeclaringType.GetTypeInfo().IsVisible())
                {
                    return false;
                }
                if (!typeInfo.IsVisible || !typeInfo.IsNestedPublic)
                {
                    return false;
                }
            }
            else
            {
                if (!typeInfo.IsVisible || !typeInfo.IsPublic)
                {
                    return false;
                }
            }
            if (typeInfo.IsGenericType && !typeInfo.IsGenericTypeDefinition)
            {
                foreach (var argument in typeInfo.GenericTypeArguments)
                {
                    if (!argument.GetTypeInfo().IsVisible())
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}