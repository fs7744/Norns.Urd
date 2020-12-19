using System.Collections.Generic;

namespace System.Linq
{
    public static partial class EnumerableExtensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> getKey)
        {
            var keys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (keys.Add(getKey(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<TSource> Union<TSource>(this TSource source, IEnumerable<TSource> sources)
        {
            yield return source;
            foreach (TSource element in sources)
            {
                yield return element;
            }
        }
    }
}