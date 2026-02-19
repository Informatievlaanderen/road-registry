namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using System;
using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net;
using GeoJSON.Net.Geometry;
using NetTopologySuite.Geometries;
using LineString = GeoJSON.Net.Geometry.LineString;
using MultiLineString = GeoJSON.Net.Geometry.MultiLineString;
using MultiPoint = GeoJSON.Net.Geometry.MultiPoint;
using Polygon = GeoJSON.Net.Geometry.Polygon;
using MultiPolygon = GeoJSON.Net.Geometry.MultiPolygon;
using Point = GeoJSON.Net.Geometry.Point;

internal static class GeoJsonNetToNtsConverter
{
    public static Geometry? ToNtsGeometry(this GeoJSONObject? geoJson, GeometryFactory? factory = null)
    {
        if (geoJson is null) return null;
        factory ??= GeometryFactory.Default;

        return geoJson switch
        {
            Point p => factory.CreatePoint(ToCoordinate(p.Coordinates)),
            LineString ls => factory.CreateLineString(ToCoordinates(ls.Coordinates)),
            MultiLineString mls => CreateMultiLineString(factory, mls),
            Polygon poly => CreatePolygon(factory, poly),
            MultiPolygon mpoly => CreateMultiPolygon(factory, mpoly),
            MultiPoint mpt => CreateMultiPoint(factory, mpt),
            _ => throw new NotSupportedException($"GeoJSON type '{geoJson.Type}' not supported")
        };
    }

    private static Coordinate ToCoordinate(IPosition pos)
    {
        var x = pos.Longitude;
        var y = pos.Latitude;
        var z = pos.Altitude ?? double.NaN;

        return double.IsNaN(z) ? new Coordinate(x, y) : new CoordinateZ(x, y, z);
    }

    private static Coordinate[] ToCoordinates(IEnumerable<IPosition> positions) =>
        positions.Select(ToCoordinate).ToArray();

    private static Geometry CreateMultiPoint(GeometryFactory factory, MultiPoint mpt)
    {
        var points = new List<NetTopologySuite.Geometries.Point>();
        if (mpt.Coordinates != null)
        {
            foreach (var point in mpt.Coordinates)
            {
                points.Add(factory.CreatePoint(ToCoordinate(point.Coordinates)));
            }
        }

        return factory.CreateMultiPoint(points.ToArray());
    }

    private static Geometry CreateMultiLineString(GeometryFactory factory, MultiLineString mls)
    {
        var lines = new List<NetTopologySuite.Geometries.LineString>();
        if (mls.Coordinates != null)
        {
            foreach (var lineString in mls.Coordinates)
            {
                lines.Add(factory.CreateLineString(ToCoordinates(lineString.Coordinates)));
            }
        }

        return factory.CreateMultiLineString(lines.ToArray());
    }

    private static NetTopologySuite.Geometries.Polygon CreatePolygon(GeometryFactory factory, Polygon poly)
    {
        var rings = new List<NetTopologySuite.Geometries.LinearRing>();
        if (poly.Coordinates != null)
        {
            foreach (var ringLine in poly.Coordinates)
            {
                rings.Add(factory.CreateLinearRing(ToCoordinates(ringLine.Coordinates)));
            }
        }

        if (rings.Count == 0)
        {
            return factory.CreatePolygon(); // empty polygon
        }

        var shell = rings[0];
        var holes = rings.Skip(1).ToArray();
        return factory.CreatePolygon(shell, holes);
    }

    private static Geometry CreateMultiPolygon(GeometryFactory factory, MultiPolygon mpoly)
    {
        var polygons = new List<NetTopologySuite.Geometries.Polygon>();
        if (mpoly.Coordinates != null)
        {
            foreach (var polygon in mpoly.Coordinates)
            {
                polygons.Add(CreatePolygon(factory, polygon));
            }
        }

        return factory.CreateMultiPolygon(polygons.ToArray());
    }
}
