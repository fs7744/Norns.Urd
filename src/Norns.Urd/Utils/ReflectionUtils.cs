using Norns.Urd.Extensions;
using System;
using System.Reflection;

namespace Norns.Urd.Utils
{
    public static class ReflectionUtils
    {
        public static bool IsVisibleAndVirtual(this MethodInfo method)
        {
            if (method == null)
            {
                throw new ArgumentNullException(nameof(method));
            }
            if (method.IsStatic || method.IsFinal)
            {
                return false;
            }
            return method.IsVirtual &&
                    (method.IsPublic || method.IsFamily || method.IsFamilyOrAssembly);
        }

        public static bool IsVoid(this MethodInfo methodInfo)
        {
            return methodInfo.ReturnType == typeof(void);
        }

        public static bool IsAsync(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            var returnType = methodInfo.ReturnType.GetTypeInfo();
            return returnType.IsTask() || returnType.IsTaskWithResult() || returnType.IsValueTask() || returnType.IsValueTaskWithResult();
        }

        public static bool IsTask(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            var returnType = methodInfo.ReturnType.GetTypeInfo();
            return returnType.IsTask();
        }

        public static bool IsValueTask(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            var returnType = methodInfo.ReturnType.GetTypeInfo();
            return returnType.IsValueTask();
        }
        
        public static bool IsReturnTask(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            var returnType = methodInfo.ReturnType.GetTypeInfo();
            return returnType.IsTaskWithResult();
        }

        public static bool IsReturnValueTask(this MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new ArgumentNullException(nameof(methodInfo));
            }
            var returnType = methodInfo.ReturnType.GetTypeInfo();
            return returnType.IsValueTaskWithResult();
        }
    }
}