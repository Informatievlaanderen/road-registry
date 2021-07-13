namespace RoadRegistry.BackOffice
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
                polygons: Array.ConvertAll(geometry.MultiPolygon, polygon => new NetTopologySuite.Geometries.Polygon(
                    shell: new NetTopologySuite.Geometries.LinearRing(
                        GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory.Create(Array.ConvertAll(polygon.Shell.Points, point => new NetTopologySuite.Geometries.Coordinate(point.X, point.Y)))
                        , GeometryConfiguration.GeometryFactory),
                    holes: Array.ConvertAll(polygon.Holes, hole => new NetTopologySuite.Geometries.LinearRing(
                        GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory.Create(Array.ConvertAll(hole.Points, point => new NetTopologySuite.Geometries.Coordinate(point.X, point.Y)))
                        , GeometryConfiguration.GeometryFactory))
                    , GeometryConfiguration.GeometryFactory))
                , GeometryConfiguration.GeometryFactory);
        }

        public static NetTopologySuite.Geometries.MultiPolygon Translate(Messages.RoadNetworkExtractGeometry geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));

            return new NetTopologySuite.Geometries.MultiPolygon(
                polygons: Array.ConvertAll(geometry.MultiPolygon, polygon => new NetTopologySuite.Geometries.Polygon(
                    shell: new NetTopologySuite.Geometries.LinearRing(
                        GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory.Create(Array.ConvertAll(polygon.Shell.Points, point => new NetTopologySuite.Geometries.Coordinate(point.X, point.Y)))
                        , GeometryConfiguration.GeometryFactory),
                    holes: Array.ConvertAll(polygon.Holes, hole => new NetTopologySuite.Geometries.LinearRing(
                            GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory.Create(Array.ConvertAll(hole.Points, point => new NetTopologySuite.Geometries.Coordinate(point.X, point.Y)))
                            , GeometryConfiguration.GeometryFactory))
                    , GeometryConfiguration.GeometryFactory))
                , GeometryConfiguration.GeometryFactory);
        }

        public static Messages.RoadNetworkExtractGeometry TranslateToRoadNetworkExtractGeometry(NetTopologySuite.Geometries.MultiPolygon geometry)
        {
            if (geometry == null) throw new ArgumentNullException(nameof(geometry));

            var polygons = new Messages.Polygon[geometry.NumGeometries];
            var polygonIndex = 0;
            foreach (var fromPolygon in geometry.Geometries.OfType<NetTopologySuite.Geometries.Polygon>())
            {
                var toShell = new Messages.Ring
                {
                    Points = new Messages.Point[fromPolygon.Shell.NumPoints]
                };
                var fromShell = fromPolygon.Shell;
                for (var shellPointIndex = 0; shellPointIndex < fromShell.NumPoints; shellPointIndex++)
                {
                    toShell.Points[shellPointIndex] = new Messages.Point
                    {
                        X = fromShell.Coordinates[shellPointIndex].X,
                        Y = fromShell.Coordinates[shellPointIndex].Y
                    };
                }

                var toHoles = new Messages.Ring[fromPolygon.Holes.Length];
                for (var holeIndex = 0; holeIndex < fromPolygon.Holes.Length; holeIndex++)
                {
                    var toHole = toHoles[holeIndex];
                    var fromHole = fromPolygon.Holes[holeIndex];
                    toHole.Points = new Messages.Point[fromHole.NumPoints];
                    for (var holePointIndex = 0; holePointIndex < fromHole.NumPoints; holePointIndex++)
                    {
                        toShell.Points[holePointIndex] = new Messages.Point
                        {
                            X = fromHole.Coordinates[holePointIndex].X,
                            Y = fromHole.Coordinates[holePointIndex].Y
                        };
                    }
                }

                polygons[polygonIndex] = new Messages.Polygon
                {
                    Shell = toShell, Holes = toHoles
                };
                polygonIndex++;
            }

            return new Messages.RoadNetworkExtractGeometry
            {
                SpatialReferenceSystemIdentifier = geometry.SRID,
                MultiPolygon = polygons
            };
        }
    }
}
