namespace RoadRegistry.Wfs.Projections
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using BackOffice.Messages;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;

    public static class WfsGeometryTranslator
    {
        public static NetTopologySuite.Geometries.LineString Translate2D(RoadSegmentGeometry geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException(nameof(geometry));

            if (geometry.MultiLineString.Length != 1)
                throw new ArgumentException(
                    $"The geometry multilinestring must contain exactly one linestring. The geometry contains {geometry.MultiLineString.Length} linestrings.",
                    nameof(geometry));

            var fromLineString = geometry.MultiLineString.Single();
            var toPoints = new List<NetTopologySuite.Geometries.Coordinate>();
            for (var index = 0; index < fromLineString.Points.Length && index < fromLineString.Measures.Length; index++)
            {
                var fromPoint = fromLineString.Points[index];
                toPoints.Add(new NetTopologySuite.Geometries.Coordinate(fromPoint.X, fromPoint.Y));
            }
            return new NetTopologySuite.Geometries.LineString(
                new NetTopologySuite.Geometries.Implementation.CoordinateArraySequence(toPoints.ToArray()),
                GeometryConfiguration.GeometryFactory)
                {
                    SRID = geometry.SpatialReferenceSystemIdentifier
                };
        }
    }
}
