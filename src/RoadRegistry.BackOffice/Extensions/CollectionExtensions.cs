using System;
using System.Collections.Generic;
using System.Linq;

namespace RoadRegistry.BackOffice
{
    public static class CollectionExtensions
    {
        public static IEnumerable<ICollection<T>> SplitIntoBatches<T>(this ICollection<T> collection, int batchCount)
        {
            var batchSize = (int)Math.Floor((double)collection.Count / batchCount) + 1;
            return Enumerable.Range(0, batchCount).Select(batchIndex => collection.Skip(batchSize * batchIndex).Take(batchSize).ToArray());
        }
    }
}
