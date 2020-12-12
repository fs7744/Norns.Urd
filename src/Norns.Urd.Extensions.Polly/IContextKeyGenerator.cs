namespace Norns.Urd.Extensions.Polly
{
    public interface IContextKeyGenerator
    {
        string GenerateKey(AspectContext context);
    }
}