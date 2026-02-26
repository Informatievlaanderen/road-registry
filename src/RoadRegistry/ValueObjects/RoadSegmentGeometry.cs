namespace RoadRegistry.ValueObjects;

using System;
using Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public sealed record RoadSegmentGeometry : GeometryObject, IEquatable<RoadSegmentGeometry>
{
    private MultiLineString? _geometry;

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public MultiLineString Value
    {
        get
        {
            _geometry ??= ((MultiLineString)new WKTReader().Read(WKT))
                .WithSrid(SRID)
                .WithMeasureOrdinates();
            return _geometry;
        }
    }

    public RoadSegmentGeometry(int srid, string wkt) : base(srid, wkt)
    {
    }

    private RoadSegmentGeometry(Geometry geometry) : base(geometry)
    {
    }

    public static RoadSegmentGeometry Create(MultiLineString geometry)
    {
        return new RoadSegmentGeometry(geometry);
    }

    public bool Equals(RoadSegmentGeometry? other)
    {
        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
