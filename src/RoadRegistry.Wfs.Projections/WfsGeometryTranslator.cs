namespace RoadRegistry.Wfs.Projections;

using System;
using System.Collections.Generic;
using System.Linq;
using BackOffice.Messages;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.BackOffice;
using LineString = NetTopologySuite.Geometries.LineString;

public static class WfsGeometryTranslator
{
    public static LineString Translate2D(RoadSegmentGeometry geometry)
    {
        if (geometry == null)
            throw new ArgumentNullException(nameof(geometry));

        if (geometry.MultiLineString.Length != 1)
            throw new ArgumentException(
                $"The geometry multilinestring must contain exactly one linestring. The geometry contains {geometry.MultiLineString.Length} linestrings.",
                nameof(geometry));

        var fromLineString = geometry.MultiLineString.Single();
        var toPoints = new List<Coordinate>();
        for (var index = 0; index < fromLineString.Points.Length && index < fromLineString.Measures.Length; index++)
        {
            var fromPoint = fromLineString.Points[index];
            toPoints.Add(new Coordinate(fromPoint.X, fromPoint.Y));
        }

        return new LineString(
            new CoordinateArraySequence(toPoints.ToArray()),
            WellKnownGeometryFactories.Default)
        {
            SRID = geometry.SpatialReferenceSystemIdentifier
        };
    }
}
