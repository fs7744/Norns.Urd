using System;

namespace Norns.Urd.Http
{
    public class OptionsCreator<T> where T : class
    {
        public T Options { get; }

        public OptionsCreator(Func<T> creator)
        {
            Options = creator?.Invoke();
        }

        public OptionsCreator() : this(null)
        {
        }
    }
}