namespace NetTopologySuite.Geometries;

using System;
using System.Linq;

public static class NetTopologySuiteExtensions
{
    public static IPolygonal[] GetPolygonals(this IPolygonal polygonal)
    {
        if (polygonal is Polygon polygon) return new IPolygonal[] { polygon };

        if (polygonal is MultiPolygon multiPolygon) return multiPolygon.Geometries.Cast<IPolygonal>().ToArray();

        throw new NotSupportedException($"Type '{polygonal.GetType().FullName}' is not supported. Only 'Polygon' or 'MultiPolygon' are allowed.");
    }
}