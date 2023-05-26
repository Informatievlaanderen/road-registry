using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using LineString = GeoJSON.Net.Geometry.LineString;
using MultiLineString = GeoJSON.Net.Geometry.MultiLineString;
using MultiPolygon = GeoJSON.Net.Geometry.MultiPolygon;
using Polygon = GeoJSON.Net.Geometry.Polygon;
using Position = GeoJSON.Net.Geometry.Position;
public static class GeoJsonExtensions
{
    public static MultiLineString ToGeoJson(this NetTopologySuite.Geometries.MultiLineString multiLineString)
    {
        return new MultiLineString(multiLineString.Geometries.Cast<NetTopologySuite.Geometries.LineString>().Select(lineString => lineString.ToGeoJson()));
    }
    public static LineString ToGeoJson(this NetTopologySuite.Geometries.LineString lineString)
    {
        return new LineString(lineString.Coordinates.Select(x => x.ToPosition()));
    }
    public static MultiPolygon ToGeoJson(this NetTopologySuite.Geometries.MultiPolygon multiPolygon)
    {
        return new MultiPolygon(multiPolygon.Geometries.Cast<NetTopologySuite.Geometries.Polygon>().Select(polygon => polygon.ToGeoJson()));
    }
    public static Polygon ToGeoJson(this NetTopologySuite.Geometries.Polygon polygon)
    {
        return new Polygon(polygon.GetRings().Select(lineString => lineString.ToGeoJson()));
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
