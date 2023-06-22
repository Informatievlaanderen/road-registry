namespace RoadRegistry.BackOffice;

using Be.Vlaanderen.Basisregisters.Shaperon;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using Messages;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NetTopologySuite.IO.GML2;
using System;
using System.Collections.Generic;
using System.Linq;
using LineString = NetTopologySuite.Geometries.LineString;
using Point = NetTopologySuite.Geometries.Point;
using Polygon = Be.Vlaanderen.Basisregisters.Shaperon.Polygon;

public static class GeometryTranslator
{
    private static readonly GeometryFactory NoSridGeometryFactory = new(GeometryConfiguration.GeometryFactory.PrecisionModel, 0, GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory);

    private static Geometry ApplyBuffer(IPolygonal geometry, double buffer)
    {
        switch (geometry)
        {
            case MultiPolygon multiPolygon:
                return multiPolygon.Buffer(buffer);
            case NetTopologySuite.Geometries.Polygon polygon:
                return polygon.Buffer(buffer);
            default:
                throw new InvalidOperationException(
                    $"The geometry must be either a polygon or a multipolygon to be able to translate it to a road network extract geometry. The geometry was a {geometry.GetType().Name}");
        }
    }

    private static object Flatten(this RoadNetworkExtractGeometry geometry)
    {
        if (geometry.MultiPolygon != null && geometry.Polygon != null) return null;

        return new object[]
        {
            geometry.MultiPolygon,
            geometry.Polygon
        }.SingleOrDefault(value => !ReferenceEquals(value, null));
    }

    public static bool GmlIsValidLineString(string gml)
    {
        try
        {
            ParseGmlLineString(gml);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static MultiLineString ParseGmlLineString(string gml)
    {
        var geometry = ParseGml(gml.ThrowIfNull());
        if (geometry == null)
        {
            throw new InvalidOperationException("The GML is invalid");
        }

        return geometry
            .ToMultiLineString(NoSridGeometryFactory)
            .WithMeasureOrdinates();
    }

    public static MultiLineString WithMeasureOrdinates(this MultiLineString multiLineString)
    {
        var lineStrings = multiLineString.Geometries
            .Cast<LineString>()
            .Select(lineString =>
            {
                if (lineString.Count == 0)
                {
                    return lineString;
                }

                var coordinates = new Coordinate[lineString.Count];
                coordinates[0] = new CoordinateM(lineString.StartPoint.X, lineString.StartPoint.Y, 0);

                var currentMeasure = coordinates[0].M;

                for (var i = 1; i < lineString.Count; i++)
                {
                    var currentPoint = lineString[i];
                    var previousPoint = lineString[i - 1];

                    var distanceToPreviousPoint = previousPoint.Distance(currentPoint);
                    currentMeasure += distanceToPreviousPoint;
                    coordinates[i] = new CoordinateM(currentPoint.X, currentPoint.Y, currentMeasure);
                }

                return new LineString(new CoordinateArraySequence(coordinates), multiLineString.Factory)
                    .WithSrid(multiLineString.SRID);
            })
            .ToArray();

        return new MultiLineString(lineStrings, multiLineString.Factory)
            .WithSrid(multiLineString.SRID);
    }

    private static Geometry ParseGml(string gml)
    {
        var gmlReader = new GMLReader(NoSridGeometryFactory);
        return gmlReader.Read(gml);
    }

    public static MultiLineString ToMultiLineString(PolyLineM polyLineM)
    {
        return Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryMultiLineString(polyLineM)
            .WithMeasureOrdinates();
    }

    public static MultiPolygon ToMultiPolygon(Polygon polygon)
    {
        try
        {
            return Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryMultiPolygon(polygon);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message == "The shell of a polygon must have a clockwise orientation.")
            {
                return Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryMultiPolygon(new Polygon(polygon.BoundingBox, polygon.Parts, polygon.Points.Reverse().ToArray()));
            }
            throw;
        }
    }

    public static Point ToPoint(Be.Vlaanderen.Basisregisters.Shaperon.Point point)
    {
        return Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryPoint(point);
    }

    public static NetTopologySuite.Geometries.Polygon ToPolygon(Polygon polygon)
    {
        return Be.Vlaanderen.Basisregisters.Shaperon.Geometries.GeometryTranslator.ToGeometryPolygon(polygon);
    }
    
    public static Point Translate(RoadNodeGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return new Point(geometry.Point.X, geometry.Point.Y)
            .WithSrid(geometry.SpatialReferenceSystemIdentifier);
    }

    public static RoadNodeGeometry Translate(Point geometry)
    {
        if (geometry == null) throw new ArgumentNullException(nameof(geometry));

        return new RoadNodeGeometry
        {
            SpatialReferenceSystemIdentifier = geometry.SRID,
            Point = new Messages.Point
            {
                X = geometry.X,
                Y = geometry.Y
            }
        };
    }

    public static MultiLineString Translate(RoadSegmentGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        var toLineStrings = new List<LineString>();
        foreach (var fromLineString in geometry.MultiLineString)
        {
            var toPoints = new List<Coordinate>();
            for (var index = 0; index < fromLineString.Points.Length && index < fromLineString.Measures.Length; index++)
            {
                var fromPoint = fromLineString.Points[index];
                var fromMeasure = fromLineString.Measures[index];
                toPoints.Add(new CoordinateM(fromPoint.X, fromPoint.Y, fromMeasure));
            }

            toLineStrings.Add(
                new LineString(
                    new CoordinateArraySequence(toPoints.ToArray()),
                    GeometryConfiguration.GeometryFactory)
                    .WithSrid(geometry.SpatialReferenceSystemIdentifier)
            );
        }

        return new MultiLineString(toLineStrings.ToArray(), GeometryConfiguration.GeometryFactory)
            .WithSrid(geometry.SpatialReferenceSystemIdentifier)
            .WithMeasureOrdinates();
    }

    public static RoadSegmentGeometry Translate(MultiLineString geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        geometry = geometry.WithMeasureOrdinates();

        var toMultiLineString = new Messages.LineString[geometry.NumGeometries];
        var lineIndex = 0;
        foreach (var fromLineString in geometry.Geometries.OfType<LineString>())
        {
            var toLineString = new Messages.LineString
            {
                Points = new Messages.Point[fromLineString.NumPoints],
                Measures = fromLineString.GetOrdinates(Ordinate.M)
            };

            for (var pointIndex = 0; pointIndex < fromLineString.NumPoints; pointIndex++)
                toLineString.Points[pointIndex] = new Messages.Point
                {
                    X = fromLineString.CoordinateSequence.GetOrdinate(pointIndex, Ordinate.X),
                    Y = fromLineString.CoordinateSequence.GetOrdinate(pointIndex, Ordinate.Y)
                };

            toMultiLineString[lineIndex] = toLineString;
            lineIndex++;
        }

        return new RoadSegmentGeometry
        {
            SpatialReferenceSystemIdentifier = geometry.SRID,
            MultiLineString = toMultiLineString
        };
    }

    public static MultiPolygon Translate(MunicipalityGeometry geometry)
    {
        if (geometry == null) throw new ArgumentNullException(nameof(geometry));

        return new MultiPolygon(
            Array.ConvertAll(geometry.MultiPolygon, polygon => new NetTopologySuite.Geometries.Polygon(
                new LinearRing(
                    GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory.Create(Array.ConvertAll(polygon.Shell.Points, point => new Coordinate(point.X, point.Y)))
                    , GeometryConfiguration.GeometryFactory),
                Array.ConvertAll(polygon.Holes, hole => new LinearRing(
                    GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory.Create(Array.ConvertAll(hole.Points, point => new Coordinate(point.X, point.Y)))
                    , GeometryConfiguration.GeometryFactory))
                , GeometryConfiguration.GeometryFactory))
            , GeometryConfiguration.GeometryFactory);
    }

    public static IPolygonal Translate(RoadNetworkExtractGeometry geometry)
    {
        if (geometry == null) throw new ArgumentNullException(nameof(geometry));

        if (geometry.WKT != null)
        {
            var geometryServices = new NtsGeometryServices(GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory, GeometryConfiguration.GeometryFactory.PrecisionModel, GeometryConfiguration.GeometryFactory.SRID);
            return (IPolygonal)new WKTReader(geometryServices).Read(geometry.WKT);
        }

        switch (geometry.Flatten())
        {
            case Messages.Polygon[] multiPolygon:
                return new MultiPolygon(
                    Array.ConvertAll(multiPolygon, polygon =>
                        new NetTopologySuite.Geometries.Polygon(
                            new LinearRing(
                                GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory.Create(
                                    Array.ConvertAll(polygon.Shell.Points,
                                        point => new Coordinate(point.X, point.Y)))
                                , GeometryConfiguration.GeometryFactory),
                            Array.ConvertAll(polygon.Holes, hole => new LinearRing(
                                GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory.Create(
                                    Array.ConvertAll(hole.Points,
                                        point => new Coordinate(point.X, point.Y)))
                                , GeometryConfiguration.GeometryFactory))
                            , GeometryConfiguration.GeometryFactory))
                    , GeometryConfiguration.GeometryFactory);
            case Messages.Polygon polygon:
                return new NetTopologySuite.Geometries.Polygon(
                    new LinearRing(
                        GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory.Create(
                            Array.ConvertAll(polygon.Shell.Points,
                                point => new Coordinate(point.X, point.Y)))
                        , GeometryConfiguration.GeometryFactory),
                    Array.ConvertAll(geometry.Polygon.Holes, hole => new LinearRing(
                        GeometryConfiguration.GeometryFactory.CoordinateSequenceFactory.Create(
                            Array.ConvertAll(hole.Points,
                                point => new Coordinate(point.X, point.Y)))
                        , GeometryConfiguration.GeometryFactory))
                    , GeometryConfiguration.GeometryFactory);
            default:
                throw new InvalidOperationException(
                    "The road network extract geometry must have either its Polygon or MultiPolygon property set (but not both) to be able to translate it to a road network extract geometry.");
        }
    }

    public static RoadNetworkExtractGeometry TranslateToRoadNetworkExtractGeometry(IPolygonal geometry, double buffer = 0)
    {
        if (geometry == null) throw new ArgumentNullException(nameof(geometry));

        var geometryWithBuffer = buffer != 0
            ? ApplyBuffer(geometry, buffer) as IPolygonal
            : geometry;

        switch (geometryWithBuffer)
        {
            case MultiPolygon multiPolygon:
                {
                    var polygons = new Messages.Polygon[multiPolygon.NumGeometries];
                    var polygonIndex = 0;
                    foreach (var fromPolygon in multiPolygon.Geometries.OfType<NetTopologySuite.Geometries.Polygon>())
                    {
                        var toShell = new Ring
                        {
                            Points = new Messages.Point[fromPolygon.Shell.NumPoints]
                        };
                        var fromShell = fromPolygon.Shell;
                        for (var shellPointIndex = 0; shellPointIndex < fromShell.NumPoints; shellPointIndex++)
                            toShell.Points[shellPointIndex] = new Messages.Point
                            {
                                X = fromShell.Coordinates[shellPointIndex].X,
                                Y = fromShell.Coordinates[shellPointIndex].Y
                            };

                        var toHoles = new Ring[fromPolygon.Holes.Length];
                        for (var holeIndex = 0; holeIndex < fromPolygon.Holes.Length; holeIndex++)
                        {
                            var fromHole = fromPolygon.Holes[holeIndex];
                            toHoles[holeIndex] = new Ring
                            {
                                Points = new Messages.Point[fromHole.NumPoints]
                            };
                            for (var holePointIndex = 0; holePointIndex < fromHole.NumPoints; holePointIndex++)
                                toHoles[holeIndex].Points[holePointIndex] = new Messages.Point
                                {
                                    X = fromHole.Coordinates[holePointIndex].X,
                                    Y = fromHole.Coordinates[holePointIndex].Y
                                };
                        }

                        polygons[polygonIndex] = new Messages.Polygon
                        {
                            Shell = toShell,
                            Holes = toHoles
                        };
                        polygonIndex++;
                    }

                    return new RoadNetworkExtractGeometry
                    {
                        SpatialReferenceSystemIdentifier = multiPolygon.SRID,
                        WKT = multiPolygon.ToText(),
                        MultiPolygon = polygons,
                        Polygon = null
                    };
                }
            case NetTopologySuite.Geometries.Polygon polygon:
                {
                    var toShell = new Ring
                    {
                        Points = new Messages.Point[polygon.Shell.NumPoints]
                    };
                    var fromShell = polygon.Shell;
                    for (var shellPointIndex = 0; shellPointIndex < fromShell.NumPoints; shellPointIndex++)
                        toShell.Points[shellPointIndex] = new Messages.Point
                        {
                            X = fromShell.Coordinates[shellPointIndex].X,
                            Y = fromShell.Coordinates[shellPointIndex].Y
                        };

                    var toHoles = new Ring[polygon.Holes.Length];
                    for (var holeIndex = 0; holeIndex < polygon.Holes.Length; holeIndex++)
                    {
                        var fromHole = polygon.Holes[holeIndex];
                        toHoles[holeIndex] = new Ring
                        {
                            Points = new Messages.Point[fromHole.NumPoints]
                        };
                        for (var holePointIndex = 0; holePointIndex < fromHole.NumPoints; holePointIndex++)
                            toHoles[holeIndex].Points[holePointIndex] = new Messages.Point
                            {
                                X = fromHole.Coordinates[holePointIndex].X,
                                Y = fromHole.Coordinates[holePointIndex].Y
                            };
                    }

                    return new RoadNetworkExtractGeometry
                    {
                        SpatialReferenceSystemIdentifier = polygon.SRID,
                        WKT = polygon.ToText(),
                        MultiPolygon = null,
                        Polygon = new Messages.Polygon
                        {
                            Shell = toShell,
                            Holes = toHoles
                        }
                    };
                }
            default:
                throw new InvalidOperationException(
                    $"The geometry must be either a polygon or a multipolygon to be able to translate it to a road network extract geometry. The geometry was a {geometry.GetType().Name}");
        }
    }

    public static T WithSrid<T>(this T geometry, int srid)
        where T: Geometry
    {
        geometry.SRID = srid > 0
            ? srid
            : GeometryConfiguration.GeometryFactory.SRID;

        return geometry;
    }
}
