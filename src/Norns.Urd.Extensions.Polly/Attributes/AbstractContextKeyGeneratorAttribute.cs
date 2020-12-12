using System;

namespace Norns.Urd.Extensions.Polly.Attributes
{
    public abstract class AbstractContextKeyGeneratorAttribute : Attribute, IContextKeyGenerator
    {
        public abstract string GenerateKey(AspectContext context);
    }
}