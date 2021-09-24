using System.Collections.Generic;

namespace DIFactoryBuilder.SourceGenerator.Extensions
{
    /// <summary>
    ///     Extensions for IEnumerable.
    /// </summary>
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T> Intersperse<T>(this IEnumerable<T> source, T element)
        {
            foreach (T value in source)
            {
                yield return value;
                yield return element;
            }
        }
    }
}