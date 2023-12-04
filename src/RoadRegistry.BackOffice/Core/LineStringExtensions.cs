namespace RoadRegistry.BackOffice.Core;

using System;
using System.Linq;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;

internal static class LineStringExtensions
{
    public static bool SelfIntersects(this LineString instance)
    {
        if (instance.Length <= 0.0 || instance.NumPoints <= 2)
            return false;

        return !instance.IsSimple;
    }

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
                            new Coordinate(Math.Round(fromPoint.X, Precisions.MeasurementPrecision), Math.Round(fromPoint.Y, Precisions.MeasurementPrecision)),
                            new Coordinate(Math.Round(toPoint.X, Precisions.MeasurementPrecision), Math.Round(toPoint.Y, Precisions.MeasurementPrecision))
                        })
                    , WellKnownGeometryFactories.Default);
            fromPoint = toPoint;
        }

        var overlappings =
            (
                from left in lines
                from right in lines
                where !ReferenceEquals(left, right)
                select new
                {
                    Left = left,
                    Right = right,
                    LeftOverlapsRight = left.Overlaps(right),
                    LeftCoversRight = left.Covers(right)
                }
            )
            .Where(x => x.LeftOverlapsRight || x.LeftCoversRight)
            .ToArray();

        return overlappings.Any();
    }

    public static bool HasInvalidMeasureOrdinates(this LineString instance)
    {
        var measures = instance.GetOrdinates(Ordinate.M);
        return measures.Any(value => double.IsNaN(value) || double.IsNegativeInfinity(value) || double.IsPositiveInfinity(value));
    }
}
