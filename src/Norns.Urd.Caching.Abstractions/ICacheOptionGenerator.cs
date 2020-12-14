namespace Norns.Urd.Caching
{
    public interface ICacheOptionGenerator
    {
        CacheOptions Generate(AspectContext context);
    }
}