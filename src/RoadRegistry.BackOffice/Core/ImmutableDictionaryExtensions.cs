namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;

    internal static class ImmutableDictionaryExtensions
    {
        public static ImmutableDictionary<TKey, TValue> TryReplace<TKey, TValue>(
            this ImmutableDictionary<TKey, TValue> dictionary,
            TKey key,
            Converter<TValue, TValue> replacer
        )
        {
            if(dictionary.TryGetValue(key, out var value))
            {
                return dictionary
                    .Remove(key)
                    .Add(key, replacer(value));
            }
            return dictionary;
        }

        public static ImmutableDictionary<TKey, IReadOnlyList<TValue>> AddOrMergeDistinct<TKey, TValue>(
            this ImmutableDictionary<TKey, IReadOnlyList<TValue>> dictionary,
            TKey key,
            IEnumerable<TValue> values)
        {
            if(dictionary.TryGetValue(key, out var mergeable))
            {
                return dictionary
                    .Remove(key)
                    .Add(key, values.Concat(mergeable).Distinct().ToArray());
            }

            return dictionary.Add(key, values.ToArray());
        }
    }
}
