namespace RoadRegistry.LegacyStreamExtraction
{
    using System.Collections.Generic;

    internal static class EnumerableExtensions
    {
        public static IEnumerable<object> Concat<TEvent1, TEvent2>(this IEnumerable<TEvent1> left, IEnumerable<TEvent2> right) 
        {
            foreach(object @event1 in left)
            {
                yield return @event1;
            }
            foreach(object @event2 in right)
            {
                yield return @event2;
            }
        }
    }
}