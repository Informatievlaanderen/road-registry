namespace RoadRegistry.BackOffice.Core;

using System;
using NetTopologySuite.Geometries;

public static class GeometryExtensions
{
    public static bool IsReasonablyEqualTo(this MultiLineString @this, MultiLineString other, VerificationContextTolerances tolerances)
    {
        if (ReferenceEquals(@this, other)) return true;
        if (@this.NumGeometries != other.NumGeometries) return false;
        for (var i = 0; i < @this.NumGeometries; i++)
        {
            var thisLineString = (LineString)@this.GetGeometryN(i);
            var otherLineString = (LineString)other.GetGeometryN(i);
            if (!thisLineString.IsReasonablyEqualTo(otherLineString, tolerances)) return false;
        }

        return true;
    }

    private static bool IsReasonablyEqualTo(this LineString @this, LineString other, VerificationContextTolerances tolerances)
    {
        if (ReferenceEquals(@this, other)) return true;
        if (@this.NumPoints != other.NumPoints) return false;
        for (var i = 0; i < @this.NumPoints; i++)
        {
            var thisPoint = @this.GetCoordinateN(i);
            var otherPoint = other.GetCoordinateN(i);
            if (!thisPoint.IsReasonablyEqualTo(otherPoint, tolerances)) return false;
        }

        return true;
    }

    public static bool IsReasonablyEqualTo(this Point @this, Point other, VerificationContextTolerances tolerances)
    {
        if (ReferenceEquals(@this, other)) return true;
        if (@this.IsEmpty && other.IsEmpty) return true;
        if (@this.IsEmpty != other.IsEmpty) return false;
        return @this.EqualsExact(other, tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyEqualTo(this Coordinate @this, Coordinate other, VerificationContextTolerances tolerances)
    {
        if (ReferenceEquals(@this, other)) return true;
        return @this.Equals2D(other, tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyEqualTo(this double value, double other, VerificationContextTolerances tolerances)
    {
        return value.IsReasonablyEqualTo(other, tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyEqualTo(this decimal value, decimal other, VerificationContextTolerances tolerances)
    {
        return value.IsReasonablyEqualTo(other, (decimal)tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyLessThan(this double value, double other, VerificationContextTolerances tolerances)
    {
        return value.IsReasonablyLessThan(other, tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyLessThan(this decimal value, decimal other, VerificationContextTolerances tolerances)
    {
        return value.IsReasonablyLessThan(other, (decimal)tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyGreaterThan(this double value, double other, VerificationContextTolerances tolerances)
    {
        return value.IsReasonablyGreaterThan(other, tolerances.GeometryTolerance);
    }

    public static bool IsReasonablyGreaterThan(this decimal value, decimal other, VerificationContextTolerances tolerances)
    {
        return value.IsReasonablyGreaterThan(other, (decimal)tolerances.GeometryTolerance);
    }
}
