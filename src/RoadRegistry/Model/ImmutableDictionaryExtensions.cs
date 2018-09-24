namespace RoadRegistry.Model
{
    using System;
    using System.Collections.Immutable;

    internal static class ImmutableDictionaryExtensions
    {
        public static ImmutableDictionary<TKey, TValue> TryReplaceValue<TKey, TValue>(
            this ImmutableDictionary<TKey, TValue> dictionary,
            TKey key,
            Converter<TValue, TValue> replacer
        )
        {
            if(dictionary.TryGetValue(key, out TValue value))
            {
                return dictionary
                    .Remove(key)
                    .Add(key, replacer(value));
            }
            return dictionary;
        }
        public static ImmutableDictionary<TKey, TValue> AddOrReplaceValue<TKey, TValue>(
            this ImmutableDictionary<TKey, TValue> dictionary,
            TKey key,
            TValue value,
            Converter<TValue, TValue> replacer
        )
        {
            if(dictionary.TryGetValue(key, out TValue replaceable))
            {
                return dictionary
                    .Remove(key)
                    .Add(key, replacer(replaceable));
            }
            return dictionary.Add(key, value);
        }
    }
}
