using System;

namespace Norns.Urd.Extensions.Polly
{
    public class ContextKeyAttribute : AbstractContextKeyGeneratorAttribute
    {
        private Func<AspectContext, string> generator;

        private string key;

        public string Key
        {
            get => key;
            set
            {
                key = value;
                generator = c => key;
            }
        }

        private Type generatorType;

        public Type GeneratorType
        {
            get => generatorType;
            set
            {
                generatorType = value;
                if (!(Activator.CreateInstance(value) is IContextKeyGenerator g))
                {
                    throw new ArgumentException("GeneratorType must be IContextKeyGenerator.");
                }
                generator = c => g.GenerateKey(c);
            }
        }

        public override string GenerateKey(AspectContext context) => generator(context);
    }
}