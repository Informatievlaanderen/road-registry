using System;

namespace RoadRegistry.BackOffice.Extensions;

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
}
