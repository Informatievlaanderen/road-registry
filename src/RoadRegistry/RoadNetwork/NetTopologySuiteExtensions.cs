namespace RoadRegistry.RoadNetwork;

using System;
using Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using ValueObjects;

public static class NetTopologySuiteExtensions
{
    public static RoadNetworkChangeGeometry ToGeometryObject(this MultiPolygon geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return RoadNetworkChangeGeometry.Create(geometry);
    }

    public static MultiPolygon ToGeometry(this RoadNetworkChangeGeometry geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return ((MultiPolygon)new WKTReader().Read(geometry.WKT)
                .WithSrid(geometry.SRID));
    }
}
