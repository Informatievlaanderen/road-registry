namespace RoadRegistry.Integration.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

public static class RoadSegmentDynamicAttributeSynchronizationBehavior
{
    public static void Synchronize<T>(this DbSet<T> source,
        Dictionary<int, T> currentSet,
        Dictionary<int, T> nextSet,
        Action<T, T> modifier) where T : class
    {
        var allKeys = new HashSet<int>(currentSet.Keys.Concat(nextSet.Keys));
        foreach (var key in allKeys)
        {
            var gotCurrent = currentSet.TryGetValue(key, out var current);
            var gotNext = nextSet.TryGetValue(key, out var next);
            if (gotCurrent && gotNext)
            {
                modifier(current, next);
            }
            else if (gotCurrent)
            {
                source.Remove(current);
                source.Local.Remove(current);
            }
            else if (gotNext)
            {
                source.Add(next);
            }
        }
    }
}