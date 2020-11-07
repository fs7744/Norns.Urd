namespace Norns.Urd
{
    public interface IAspectConfiguration
    {
        InterceptorCollection Interceptors { get; }
        InterceptorFilterCollection Filters { get; }
    }
}