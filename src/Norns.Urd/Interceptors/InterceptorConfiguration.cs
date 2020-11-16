using System.Collections.Generic;

namespace Norns.Urd
{
    public interface IAspectConfiguration
    {
        List<IInterceptor> GlobalInterceptors { get; }
    }

    public class AspectConfiguration : IAspectConfiguration
    {
        public List<IInterceptor> GlobalInterceptors { get; } = new List<IInterceptor>();
    }
}