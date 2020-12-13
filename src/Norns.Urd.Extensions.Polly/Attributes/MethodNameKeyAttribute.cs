namespace Norns.Urd.Extensions.Polly
{
    public class MethodNameKeyAttribute : AbstractContextKeyGeneratorAttribute
    {
        public override string GenerateKey(AspectContext context) => context.Method.Name;
    }
}