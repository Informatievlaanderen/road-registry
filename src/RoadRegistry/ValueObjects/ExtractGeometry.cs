namespace RoadRegistry.ValueObjects;

using System;
using Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public sealed record ExtractGeometry : GeometryObject, IEquatable<ExtractGeometry>
{
    private MultiPolygon? _geometry;

    [Newtonsoft.Json.JsonIgnore]
    [System.Text.Json.Serialization.JsonIgnore]
    public MultiPolygon Value
    {
        get
        {
            _geometry ??= ((MultiPolygon)new WKTReader().Read(WKT))
                .WithSrid(SRID);
            return _geometry;
        }
    }

    public ExtractGeometry(int srid, string wkt) : base(srid, wkt)
    {
    }

    private ExtractGeometry(Geometry geometry) : base(geometry)
    {
    }

    public static ExtractGeometry Create(MultiPolygon geometry)
    {
        return new ExtractGeometry(geometry);
    }

    public bool Equals(ExtractGeometry? other)
    {
        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}
