namespace RoadRegistry.BackOffice.Core;

using NetTopologySuite.Geometries;

//TODO-rik unit tests voor op en onder de tolerance van 0.001
internal static class GeometryExtensions
{
    public static bool EqualsWithinTolerance(this MultiLineString @this, MultiLineString other, VerificationContextTolerances tolerances)
    {
        if (ReferenceEquals(@this, other)) return true;
        if (@this.NumGeometries != other.NumGeometries) return false;
        for (var i = 0; i < @this.NumGeometries; i++)
        {
            var thisLineString = (LineString)@this.GetGeometryN(i);
            var otherLineString = (LineString)other.GetGeometryN(i);
            if (!thisLineString.EqualsWithinTolerance(otherLineString, tolerances)) return false;
        }

        return true;
    }

    private static bool EqualsWithinTolerance(this LineString @this, LineString other, VerificationContextTolerances tolerances)
    {
        if (ReferenceEquals(@this, other)) return true;
        if (@this.NumPoints != other.NumPoints) return false;
        for (var i = 0; i < @this.NumPoints; i++)
        {
            var thisPoint = @this.GetCoordinateN(i);
            var otherPoint = other.GetCoordinateN(i);
            if (!thisPoint.EqualsWithinTolerance(otherPoint, tolerances)) return false;
        }

        return true;
    }

    public static bool EqualsWithinTolerance(this Point @this, Point other, VerificationContextTolerances tolerances)
    {
        if (ReferenceEquals(@this, other)) return true;
        if (@this.IsEmpty && other.IsEmpty) return true;
        if (@this.IsEmpty != other.IsEmpty) return false;
        return @this.Distance(other) < tolerances.GeometryTolerance;
    }

    public static bool EqualsWithinTolerance(this Coordinate @this, Coordinate other, VerificationContextTolerances tolerances)
    {
        if (ReferenceEquals(@this, other)) return true;
        return @this.Distance(other) < tolerances.GeometryTolerance;
    }
}
