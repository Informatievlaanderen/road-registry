namespace System;

public static class NumericExtensions
{
    public static bool IsReasonablyEqualTo(this double value, double other, double tolerance)
    {
        return IsReasonablyEqualTo((decimal)value, (decimal)other, (decimal)tolerance);
    }

    public static bool IsReasonablyEqualTo(this decimal value, decimal other, decimal tolerance)
    {
        return Math.Abs(value - other) <= tolerance;
    }

    public static bool IsReasonablyLessThan(this double value, double other, double tolerance)
    {
        return IsReasonablyLessThan((decimal)value, (decimal)other, (decimal)tolerance);
    }

    public static bool IsReasonablyLessThan(this decimal value, decimal other, decimal tolerance)
    {
        return other - value > tolerance;
    }
}
