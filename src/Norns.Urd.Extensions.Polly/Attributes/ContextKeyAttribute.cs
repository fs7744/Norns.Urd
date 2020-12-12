using Norns.Urd.Extensions.Polly.Attributes;
using System;

namespace Norns.Urd.Extensions.Polly
{
    public class ContextKeyAttribute : AbstractContextKeyGeneratorArrtibute
    {
        private readonly Func<AspectContext, string> generator;
        public string Key { get; set; }
        public Type GeneratorType { get; set; }

        public ContextKeyAttribute()
        {
            if (GeneratorType != null)
            {
                if (!(Activator.CreateInstance(GeneratorType) is IContextKeyGenerator g))
                {
                    throw new ArgumentException("GeneratorType must be IContextKeyGenerator.");
                }
                generator = c => g.GenerateKey(c);
            }
            else if (string.IsNullOrWhiteSpace(Key))
            {
                throw new ArgumentException("Key must be not Null and not WhiteSpace.");
            }
            else
            {
                generator = c => Key;
            }
        }

        public override string GenerateKey(AspectContext context) => generator(context);
    }
}