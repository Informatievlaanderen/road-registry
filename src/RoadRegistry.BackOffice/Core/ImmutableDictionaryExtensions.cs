namespace RoadRegistry.BackOffice.Core;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

internal static class ImmutableDictionaryExtensions
{
    public static ImmutableDictionary<TKey, IReadOnlyList<TValue>> Merge<TKey, TValue>(
        this ImmutableDictionary<TKey, IReadOnlyList<TValue>> dictionary,
        TKey key,
        IEnumerable<TValue>? values)
    {
        if (values is not null)
        {
            if (dictionary.TryGetValue(key, out var mergeable))
                return dictionary
                    .Remove(key)
                    .Add(key, values.Concat(mergeable).Distinct().ToArray());

            return dictionary.Add(key, values.Distinct().ToArray());
        }

        return dictionary;
    }

    public static void Merge<TKey, TValue>(this ImmutableDictionary<TKey, IReadOnlyList<TValue>>.Builder dictionary,
        TKey key,
        IEnumerable<TValue> values)
    {
        if (dictionary.TryGetValue(key, out var mergeable))
            dictionary[key] = values.Concat(mergeable).Distinct().ToArray();
        else
            dictionary.Add(key, values.Distinct().ToArray());
    }

    public static ImmutableDictionary<TKey, TValue> TryReplace<TKey, TValue>(
        this ImmutableDictionary<TKey, TValue> dictionary,
        TKey key,
        Converter<TValue, TValue> replacer
    )
    {
        if (dictionary.TryGetValue(key, out var value))
            return dictionary
                .Remove(key)
                .Add(key, replacer(value));
        return dictionary;
    }

    public static ImmutableDictionary<TKey, TValue> TryReplace<TKey, TValue>(
        this ImmutableDictionary<TKey, TValue> dictionary,
        TKey? key,
        Converter<TValue, TValue> replacer
    )
        where TKey : struct
    {
        if (key is not null && dictionary.TryGetValue(key.Value, out var value))
            return dictionary
                .Remove(key.Value)
                .Add(key.Value, replacer(value));

        return dictionary;
    }

    public static ImmutableDictionary<TKey, TValue> TryReplaceIf<TKey, TValue>(
        this ImmutableDictionary<TKey, TValue> dictionary,
        TKey key,
        Predicate<TValue> predicate,
        Converter<TValue, TValue> replacer
    )
    {
        if (dictionary.TryGetValue(key, out var value) && predicate(value))
            return dictionary
                .Remove(key)
                .Add(key, replacer(value));
        return dictionary;
    }

    public static ImmutableDictionary<TKey, TValue>.Builder TryReplaceIf<TKey, TValue>(
        this ImmutableDictionary<TKey, TValue>.Builder dictionary,
        TKey key,
        Predicate<TValue> predicate,
        Converter<TValue, TValue> replacer
    )
    {
        if (dictionary.TryGetValue(key, out var value) && predicate(value)) dictionary[key] = replacer(value);

        return dictionary;
    }
}
