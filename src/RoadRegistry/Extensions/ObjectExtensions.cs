namespace System;

using Globalization;
using Runtime.CompilerServices;

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
