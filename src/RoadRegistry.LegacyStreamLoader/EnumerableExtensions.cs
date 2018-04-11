namespace RoadRegistry.LegacyStreamLoader
{
    using System;
    using System.Collections.Generic;

    public static class EnumerableExtensions
    {
        public static IEnumerable<T[]> Batch<T>(this IEnumerable<T> enumerable, int size)
        {
            if(size < 1) 
                throw new ArgumentOutOfRangeException(nameof(size), size, "The batch size needs to be greater than or equal to 1.");

            using(var enumerator = enumerable.GetEnumerator())
            {
                var moved = enumerator.MoveNext();
                while(moved)
                {
                    var batch = new T[size];
                    var index = 0;
                    while(moved && index < size)
                    {
                        batch[index] = enumerator.Current;
                        index++;
                        moved = enumerator.MoveNext();
                    }
                    yield return batch;
                }
            }
        }
    }
}
