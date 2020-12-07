namespace RoadRegistry.BackOffice.Core
{
    internal static class GeometryExtensions
    {
        public static bool EqualsWithinTolerance(this NetTopologySuite.Geometries.MultiLineString @this, NetTopologySuite.Geometries.MultiLineString other, double tolerance)
        {
            if (ReferenceEquals(@this, other)) return true;
            if (@this.NumGeometries != other.NumGeometries) return false;
            for (var i = 0; i < @this.NumGeometries; i++)
            {
                var thisLineString = (NetTopologySuite.Geometries.LineString)@this.GetGeometryN(i);
                var otherLineString = (NetTopologySuite.Geometries.LineString) other.GetGeometryN(i);
                if (!thisLineString.EqualsWithinTolerance(otherLineString, tolerance)) return false;
            }
            return true;
        }

        private static bool EqualsWithinTolerance(this NetTopologySuite.Geometries.LineString @this, NetTopologySuite.Geometries.LineString other, double tolerance)
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

        public static bool EqualsWithinTolerance(this NetTopologySuite.Geometries.Point @this, NetTopologySuite.Geometries.Point other, double tolerance)
        {
            if (ReferenceEquals(@this, other)) return true;
            if (@this.IsEmpty && other.IsEmpty) return true;
            if (@this.IsEmpty != other.IsEmpty) return false;
            return @this.Distance(other) <= tolerance;
        }
    }
}
