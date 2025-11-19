namespace RoadRegistry.RoadNetwork;

using System;
using NetTopologySuite.Geometries;
using ValueObjects;

public static class NetTopologySuiteExtensions
{
    public static GeometryObject ToGeometryObject(this MultiPolygon geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return new GeometryObject(geometry.SRID, geometry.AsText());
    }
}
