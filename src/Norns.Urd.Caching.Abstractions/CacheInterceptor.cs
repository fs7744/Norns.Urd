using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Norns.Urd;
using Norns.Urd.Reflection;

namespace Norns.Urd.Caching
{
    public class CacheInterceptor : AbstractInterceptor
    {
        public override int Order { get; set; } = -95000;

        public override bool CanAspect(MethodReflector method)
        {
            return method.IsDefined<CacheAttribute>();
        }

        public override void Invoke(AspectContext context, AspectDelegate next)
        {
            base.Invoke(context, next);
        }

        public override Task InvokeAsync(AspectContext context, AsyncAspectDelegate next)
        {
            throw new NotImplementedException();
        }
    }
}
