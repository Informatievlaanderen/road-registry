namespace RoadRegistry.Extensions;

using System;

public static class NumericExtensions
{
    public static bool IsReasonablyEqualTo(this RoadSegmentPositionV2 value, RoadSegmentPositionV2 other)
    {
        return IsReasonablyEqualTo(value, other.ToDouble());
    }
    public static bool IsReasonablyEqualTo(this RoadSegmentPositionV2 value, double other)
    {
        return IsReasonablyEqualTo(value.ToDouble(), other, DefaultTolerances.GeometryToleranceV2);
    }

    public static bool IsReasonablyEqualTo(this double value, double other, double tolerance)
    {
        return RoundValue(Math.Abs(value - other), tolerance) <= tolerance;
    }
    public static bool IsReasonablyEqualTo(this decimal value, decimal other, decimal tolerance)
    {
        return Math.Abs(value - other) <= tolerance;
    }

    public static bool IsReasonablyLessThan(this double value, double other, double tolerance)
    {
        return value < other && !IsReasonablyEqualTo(value, other, tolerance);
    }
    public static bool IsReasonablyLessThan(this decimal value, decimal other, decimal tolerance)
    {
        return other - value > tolerance;
    }

    public static bool IsReasonablyLessOrEqualThan(this double value, double other, double tolerance)
    {
        return value < other || IsReasonablyEqualTo(value, other, tolerance);
    }

    public static bool IsReasonablyGreaterThan(this double value, double other, double tolerance)
    {
        return value > other && !IsReasonablyEqualTo(value, other, tolerance);
    }
    public static bool IsReasonablyGreaterThan(this decimal value, decimal other, decimal tolerance)
    {
        return value - other > tolerance;
    }

    public static bool IsReasonablyGreaterOrEqualThan(this double value, double other, double tolerance)
    {
        return value > other || IsReasonablyEqualTo(value, other, tolerance);
    }

    private static int CalculateRoundPrecision(double tolerance)
    {
        return (1 / tolerance).ToInvariantString().Length + 1;
    }

    private static double RoundValue(double value, double tolerance)
    {
        var precision = CalculateRoundPrecision(tolerance);
        return Math.Round(value, precision);
    }
}
