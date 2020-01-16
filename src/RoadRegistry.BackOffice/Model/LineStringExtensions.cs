namespace RoadRegistry.BackOffice.Model
{
    using System;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.Geometries.Implementation;

    internal static class LineStringExtensions
    {
        public static bool SelfOverlaps(this LineString instance)
        {
            if (instance.Length <= 0.0 || instance.NumPoints <= 2)
                return false;

            var lines = new LineString[instance.NumPoints - 1];
            var fromPoint = instance.StartPoint;
            for (var index = 1; index < instance.NumPoints; index++)
            {
                var toPoint = instance.GetPointN(index);
                lines[index - 1] =
                    new LineString(
                        new CoordinateArraySequence(
                            new[]
                            {
                                new Coordinate(Math.Round(fromPoint.X, 3), Math.Round(fromPoint.Y, 3)),
                                new Coordinate(toPoint.X, toPoint.Y)
                            })
                        , GeometryConfiguration.GeometryFactory);
                fromPoint = toPoint;
            }

            return
            (
                from left in lines
                from right in lines
                where !ReferenceEquals(left, right)
                select left.Overlaps(right) || left.Covers(right)
            ).Any(overlaps => overlaps);
        }

        public static bool SelfIntersects(this LineString instance)
        {
            if (instance.Length <= 0.0 || instance.NumPoints <= 2)
                return false;

            return !instance.IsSimple;
        }
    }
}
