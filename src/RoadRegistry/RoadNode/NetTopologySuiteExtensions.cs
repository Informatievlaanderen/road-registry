namespace RoadRegistry.RoadNode;

using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadNetwork.ValueObjects;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadSegment.ValueObjects;
using LineString = NetTopologySuite.Geometries.LineString;

public static class NetTopologySuiteExtensions
{
    public static Point ToPoint(this GeometryObject geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return ((Point)new WKTReader().Read(geometry.WKT)
            .WithSrid(geometry.SRID));
    }

    public static GeometryObject ToGeometryObject(this Point geometry)
    {
        ArgumentNullException.ThrowIfNull(geometry);

        return new GeometryObject(geometry.SRID, geometry.AsText());
    }
}
