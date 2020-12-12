namespace Norns.Urd.Extensions.Polly.Attributes
{
    public class MethodNameKeyAttribute : AbstractContextKeyGeneratorArrtibute
    {
        public override string GenerateKey(AspectContext context) => context.Method.Name;
    }
}