using System;

namespace Norns.Urd
{
    public static class AspectContextExtensions
    {
        public static Exception GetException(this AspectContext context)
        {
            return context.AdditionalData["Exception"] as Exception;
        }

        public static void SetException(this AspectContext context, Exception exception)
        {
            context.AdditionalData["Exception"] = exception;
        }
    }
}