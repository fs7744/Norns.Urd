using System.Collections.Generic;

namespace System.Linq
{
    public static partial class EnumerableExtensions
    {
#if NET6_0_OR_GREATER
#else
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

        public static TSource FirstOrDefault<TSource>(this IEnumerable<TSource> sources, TSource defaultValue)
        {
            var enumetrator = sources.GetEnumerator();
            if (enumetrator.MoveNext())
            {
                return enumetrator.Current;
            }
            else
            {
                return defaultValue;
            }
        }
#endif

        public static IEnumerable<TSource> Union<TSource>(this TSource source, IEnumerable<TSource> sources)
        {
            yield return source;
            foreach (TSource element in sources)
            {
                yield return element;
            }
        }

        public static bool IsNullOrEmpty<TSource>(this IEnumerable<TSource> sources)
        {
            return sources == null || !sources.GetEnumerator().MoveNext();
        }
    }
}