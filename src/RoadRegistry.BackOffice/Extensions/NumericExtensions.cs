namespace System;

public static class NumericExtensions
{
    public static bool EqualsWithTolerance(this double value, double other, double tolerance)
    {
        return Math.Abs(value - other) <= tolerance;
    }

    public static bool EqualsWithTolerance(this decimal value, decimal other, decimal tolerance)
    {
        return Math.Abs(value - other) <= tolerance;
    }
}
