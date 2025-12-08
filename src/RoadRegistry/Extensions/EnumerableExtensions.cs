using System.Collections.Generic;
using System.Linq;

namespace RoadRegistry.BackOffice;

public static class EnumerableExtensions
{
    public static IEnumerable<T> ExcludeFirstAndLast<T>(this IEnumerable<T> source)
    {
        var items = source.ToList();
        items.RemoveAt(0);
        items.RemoveAt(items.Count - 1);
        return items;
    }
}
