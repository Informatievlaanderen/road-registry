namespace RoadRegistry.BackOffice.Core;

using NetTopologySuite.Geometries;

internal static class GeometryExtensions
{
    public static bool EqualsWithinTolerance(this MultiLineString @this, MultiLineString other, double tolerance)
    {
        if (ReferenceEquals(@this, other)) return true;
        if (@this.NumGeometries != other.NumGeometries) return false;
        for (var i = 0; i < @this.NumGeometries; i++)
        {
            var thisLineString = (LineString)@this.GetGeometryN(i);
            var otherLineString = (LineString)other.GetGeometryN(i);
            if (!thisLineString.EqualsWithinTolerance(otherLineString, tolerance)) return false;
        }

        return true;
    }

    private static bool EqualsWithinTolerance(this LineString @this, LineString other, double tolerance)
    {
        if (ReferenceEquals(@this, other)) return true;
        if (@this.NumPoints != other.NumPoints) return false;
        for (var i = 0; i < @this.NumPoints; i++)
        {
            var thisPoint = @this.GetCoordinateN(i);
            var otherPoint = other.GetCoordinateN(i);
            if (thisPoint.Distance(otherPoint) > tolerance) return false;
        }

        return true;
    }

    public static bool EqualsWithinTolerance(this Point @this, Point other, double tolerance)
    {
        if (ReferenceEquals(@this, other)) return true;
        if (@this.IsEmpty && other.IsEmpty) return true;
        if (@this.IsEmpty != other.IsEmpty) return false;
        return @this.Distance(other) <= tolerance;
    }
}
