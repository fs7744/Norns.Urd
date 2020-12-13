using System;

namespace Norns.Urd.Extensions.Polly
{
    public abstract class AbstractContextKeyGeneratorAttribute : Attribute, IContextKeyGenerator
    {
        public abstract string GenerateKey(AspectContext context);
    }
}