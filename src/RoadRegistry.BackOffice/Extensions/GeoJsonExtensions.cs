namespace RoadRegistry.BackOffice.Extensions;

using System.Collections.Generic;
using System.Linq;
using GeoJSON.Net.Geometry;
using NetTopologySuite.Geometries;
using LineString = GeoJSON.Net.Geometry.LineString;
using MultiLineString = GeoJSON.Net.Geometry.MultiLineString;
using MultiPolygon = GeoJSON.Net.Geometry.MultiPolygon;
using Polygon = GeoJSON.Net.Geometry.Polygon;
using Position = GeoJSON.Net.Geometry.Position;

public static class GeoJsonExtensions
{
    public static MultiPolygon ToGeoJson(this NetTopologySuite.Geometries.MultiPolygon geometry)
    {
        return new MultiPolygon(geometry.Geometries.Cast<NetTopologySuite.Geometries.Polygon>().Select(polygon => polygon.ToGeoJson()));
    }
    public static Polygon ToGeoJson(this NetTopologySuite.Geometries.Polygon geometry)
    {
        return new Polygon(geometry.GetRings().Select(lineString => lineString.ToGeoJson()));
    }
    public static MultiLineString ToGeoJson(this NetTopologySuite.Geometries.MultiLineString geometry)
    {
        return new MultiLineString(geometry.Geometries.Cast<NetTopologySuite.Geometries.LineString>().Select(lineString => lineString.ToGeoJson()));
    }
    public static LineString ToGeoJson(this NetTopologySuite.Geometries.LineString geometry)
    {
        return new LineString(geometry.Coordinates.Select(x => x.ToPosition()));
    }
    
    public static NetTopologySuite.Geometries.MultiPolygon ToNetTopologySuiteGeometry(this MultiPolygon geometry)
    {
        return new NetTopologySuite.Geometries.MultiPolygon(
            geometry.Coordinates.Select(polygon =>
                new NetTopologySuite.Geometries.Polygon(
                    new LinearRing(
                        polygon.Coordinates
                            .SelectMany(linestring => linestring.Coordinates)
                            .Select(coordinate => new Coordinate(coordinate.Longitude, coordinate.Latitude))
                            .ToArray()
                    ))
            ).ToArray());
    }

    public static double[][][] ToCoordinateArray(this MultiLineString geometry)
    {
        return geometry.Coordinates.Select(x1 => x1.ToCoordinateArray()).ToArray();
    }

    public static double[][] ToCoordinateArray(this LineString geometry)
    {
        return geometry.Coordinates.Select(x1 => x1.ToCoordinateArray()).ToArray();
    }

    public static double[] ToCoordinateArray(this IPosition position)
    {
        return new[] { position.Longitude, position.Latitude };
    }

    private static IEnumerable<NetTopologySuite.Geometries.LineString> GetRings(this NetTopologySuite.Geometries.Polygon polygon)
    {
        yield return polygon.ExteriorRing;
        foreach (var ring in polygon.InteriorRings)
        {
            yield return ring;
        }
    }
    private static Position ToPosition(this Coordinate coordinate)
    {
        return new Position(coordinate.Y, coordinate.X);
    }
}
