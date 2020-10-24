namespace Norns.Urd
{
    public class AspectConfiguration : IAspectConfiguration
    {
        public InterceptorCollection Interceptors { get; } = new InterceptorCollection();

        public InterceptorFilterCollection Filters { get; } = new InterceptorFilterCollection();
    }
}