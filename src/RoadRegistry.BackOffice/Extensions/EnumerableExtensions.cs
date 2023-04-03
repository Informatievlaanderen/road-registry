using System;
using System.Collections.Generic;
using System.Linq;

namespace RoadRegistry.BackOffice
{
    public static class EnumerableExtensions
    {
        public static bool HasNone<TSource>(this IEnumerable<TSource> source)
        {
            ArgumentNullException.ThrowIfNull(source);
            return !source.Any();
        }

        public static bool HasNone<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(predicate);
            return !source.Any(predicate);
        }
    }
}
