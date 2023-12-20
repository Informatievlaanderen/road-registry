namespace System;

internal static class NumericExtensions
{
    public static bool IsReasonablyEqualTo(this double value, double other, double tolerance)
    {
        return Math.Abs(value - other) <= tolerance;
    }

    public static bool IsReasonablyEqualTo(this decimal value, decimal other, decimal tolerance)
    {
        return Math.Abs(value - other) <= tolerance;
    }

    public static bool IsReasonablyLessThan(this double value, double other, double tolerance)
    {
        return other - value > tolerance;
    }

    public static bool IsReasonablyLessThan(this decimal value, decimal other, decimal tolerance)
    {
        return other - value > tolerance;
    }

    public static bool IsReasonablyGreaterThan(this double value, double other, double tolerance)
    {
        return value - other > tolerance;
    }

    public static bool IsReasonablyGreaterThan(this decimal value, decimal other, decimal tolerance)
    {
        return value - other > tolerance;
    }
}
