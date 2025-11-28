namespace RoadRegistry.Extensions;

using System;
using System.Globalization;
using System.Runtime.CompilerServices;

public static class ObjectExtensions
{
    public static T ThrowIfNull<T>(this T argument, [CallerArgumentExpression("argument")] string argumentName = null)
    {
        ArgumentNullException.ThrowIfNull(argument, argumentName);

        return argument;
    }

    public static string ToInvariantString(this object value)
    {
        return ToString(value, CultureInfo.InvariantCulture);
    }

    public static string ToString(this object value, IFormatProvider formatProvider)
    {
        ArgumentNullException.ThrowIfNull(value);

        return string.Format(formatProvider, "{0}", value);
    }
}
