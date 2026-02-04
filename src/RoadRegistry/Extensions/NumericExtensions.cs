namespace RoadRegistry.Extensions;

using System;

public static class NumericExtensions
{
    public static bool IsReasonablyEqualTo(this RoadSegmentPosition value, double other)
    {
        return IsReasonablyEqualTo(value.ToDouble(), other, DefaultTolerances.GeometryTolerance);
    }

    public static bool IsReasonablyEqualTo(this RoadSegmentPositionV2 value, RoadSegmentPositionV2 other)
    {
        return IsReasonablyEqualTo(value, other.ToDouble());
    }
    public static bool IsReasonablyEqualTo(this RoadSegmentPositionV2 value, double other)
    {
        return IsReasonablyEqualTo(value.ToDouble(), other, DefaultTolerances.GeometryToleranceV2);
    }
    public static RoadSegmentPositionV2 RoundToCm(this RoadSegmentPositionV2 value)
    {
        return new RoadSegmentPositionV2(value.ToDouble().RoundToCm());
    }

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
