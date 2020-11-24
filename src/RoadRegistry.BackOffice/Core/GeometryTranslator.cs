namespace RoadRegistry.BackOffice.Core
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;

    public static class GeometryTranslator
    {
        public static NetTopologySuite.Geometries.Point Translate(Messages.RoadNodeGeometry geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));

            return new NetTopologySuite.Geometries.Point(geometry.Point.X, geometry.Point.Y)
            {
                SRID = geometry.SpatialReferenceSystemIdentifier
            };
        }

        public static Messages.RoadNodeGeometry Translate(NetTopologySuite.Geometries.Point geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));

            return new Messages.RoadNodeGeometry
            {
                SpatialReferenceSystemIdentifier = geometry.SRID,
                Point = new Messages.Point
                {
                    X = geometry.X,
                    Y = geometry.Y
                }
            };
        }

        public static NetTopologySuite.Geometries.MultiLineString Translate(Messages.RoadSegmentGeometry geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));

            var toLineStrings = new List<NetTopologySuite.Geometries.LineString>();
            foreach (var fromLineString in geometry.MultiLineString)
            {
                var toPoints = new List<NetTopologySuite.Geometries.Coordinate>();
                for (var index = 0; index < fromLineString.Points.Length && index < fromLineString.Measures.Length; index++)
                {
                    var fromPoint = fromLineString.Points[index];
                    var fromMeasure = fromLineString.Measures[index];
                    toPoints.Add(new NetTopologySuite.Geometries.CoordinateM(fromPoint.X, fromPoint.Y,fromMeasure));
                }

                toLineStrings.Add(
                    new NetTopologySuite.Geometries.LineString(
                        new NetTopologySuite.Geometries.Implementation.CoordinateArraySequence(toPoints .ToArray()),
                        GeometryConfiguration.GeometryFactory)
                    {
                        SRID = geometry.SpatialReferenceSystemIdentifier
                    }
                );
            }

            return new NetTopologySuite.Geometries.MultiLineString(
                toLineStrings.ToArray(),
                GeometryConfiguration.GeometryFactory);
        }

        public static Messages.RoadSegmentGeometry Translate(NetTopologySuite.Geometries.MultiLineString geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));

            var toMultiLineString = new Messages.LineString[geometry.NumGeometries];
            var lineIndex = 0;
            foreach (var fromLineString in geometry.Geometries.OfType<NetTopologySuite.Geometries.LineString>())
            {
                var toLineString = new Messages.LineString
                {
                    Points = new Messages.Point[fromLineString.NumPoints],
                    Measures = fromLineString.GetOrdinates(NetTopologySuite.Geometries.Ordinate.M)
                };

                for (var pointIndex = 0; pointIndex < fromLineString.NumPoints; pointIndex++)
                {
                    toLineString.Points[pointIndex] = new Messages.Point
                    {
                        X = fromLineString.CoordinateSequence.GetOrdinate(pointIndex, NetTopologySuite.Geometries.Ordinate.X),
                        Y = fromLineString.CoordinateSequence.GetOrdinate(pointIndex, NetTopologySuite.Geometries.Ordinate.Y)
                    };
                }

                toMultiLineString[lineIndex] = toLineString;
                lineIndex++;
            }
            return new Messages.RoadSegmentGeometry
            {
                SpatialReferenceSystemIdentifier = geometry.SRID,
                MultiLineString = toMultiLineString
            };
        }

        public static NetTopologySuite.Geometries.MultiPolygon Translate(Messages.MunicipalityGeometry geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));

            return new NetTopologySuite.Geometries.MultiPolygon(
                Array.ConvertAll(geometry.Polygon, ring =>
                    new NetTopologySuite.Geometries.Polygon(
                        new NetTopologySuite.Geometries.LinearRing(Array.ConvertAll(ring.Points, point =>
                            new NetTopologySuite.Geometries.Coordinate(point.X, point.Y))))));
        }
    }
}
