namespace RoadRegistry.Editor.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;

    public static class RoadSegmentDynamicAttributeSynchronizationBehavior
    {
        public static async Task Synchronize<T>(this DbSet<T> source,
            Dictionary<int, T> currentSet,
            Dictionary<int, T> nextSet,
            Action<T, T> modifier) where T : class
        {
            var allKeys = new HashSet<int>(currentSet.Keys.Concat(nextSet.Keys));
            foreach (var key in allKeys)
            {
                if (currentSet.TryGetValue(key, out var current) && nextSet.TryGetValue(key, out var next))
                {
                    modifier(current, next);
                }
                else if (currentSet.TryGetValue(key, out var removal))
                {
                    source.Remove(removal);
                    source.Local.Remove(removal);
                }
                else if (nextSet.TryGetValue(key, out var addition))
                {
                    await source.AddAsync(addition);
                }
            }
        }
    }
}
