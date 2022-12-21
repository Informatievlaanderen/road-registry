namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public static class RoadSegmentDynamicAttributeSynchronizationBehavior
{
    public static ICollection<T> Synchronize<T>(this DbSet<T> source,
        Dictionary<int, T> currentSet,
        Dictionary<int, T> nextSet,
        Action<T, T> modifier,
        Action<T> remove) where T : class
    {
        var allItems = new List<T>();

        var allKeys = new HashSet<int>(currentSet.Keys.Concat(nextSet.Keys));
        foreach (var key in allKeys)
        {
            var gotCurrent = currentSet.TryGetValue(key, out var current);
            var gotNext = nextSet.TryGetValue(key, out var next);
            if (gotCurrent && gotNext)
            {
                modifier(current, next);
                allItems.Add(next);
            }
            else if (gotCurrent)
            {
                remove(current);
                allItems.Add(current);
            }
            else if (gotNext)
            {
                source.Add(next);
                allItems.Add(next);
            }
        }

        return allItems;
    }
}
