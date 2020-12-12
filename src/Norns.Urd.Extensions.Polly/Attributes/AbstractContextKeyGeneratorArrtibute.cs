using System;

namespace Norns.Urd.Extensions.Polly.Attributes
{
    public abstract class AbstractContextKeyGeneratorArrtibute : Attribute, IContextKeyGenerator
    {
        public abstract string GenerateKey(AspectContext context);
    }
}