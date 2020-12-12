namespace Norns.Urd.Extensions.Polly.Attributes
{
    public class MethodNameKeyAttribute : AbstractContextKeyGeneratorAttribute
    {
        public override string GenerateKey(AspectContext context) => context.Method.Name;
    }
}