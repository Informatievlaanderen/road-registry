namespace RoadRegistry.ValueObjects;

using System;
using Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public sealed record RoadNodeGeometry : GeometryObject, IEquatable<RoadNodeGeometry>
{
    private Point? _geometry;

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public Point Value
    {
        get
        {
            _geometry ??= (Point)new WKTReader().Read(WKT)
                .WithSrid(SRID);
            return _geometry;
        }
    }

    public RoadNodeGeometry(int srid, string wkt) : base(srid, wkt)
    {
    }

    private RoadNodeGeometry(Geometry geometry) : base(geometry)
    {
    }

    public static RoadNodeGeometry Create(Point geometry)
    {
        return new RoadNodeGeometry(geometry);
    }

    public bool Equals(RoadNodeGeometry? other)
    {
        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
