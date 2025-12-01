namespace RoadRegistry.Extensions;

using System;

public static class StringExtensions
{
    public static string NullIfEmpty(this string value)
    {
        return string.IsNullOrEmpty(value) ? null : value;
    }

    public static string WithMaxLength(this string value, int maxLength)
    {
        return value?.Substring(0, Math.Min(value.Length, maxLength));
    }

    public static bool ContainsWhitespace(this string value)
    {
        return value is not null && value.Contains(" ");
    }
}
