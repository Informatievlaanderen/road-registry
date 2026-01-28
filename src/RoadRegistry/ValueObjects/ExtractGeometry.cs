namespace RoadRegistry.ValueObjects;

using Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public record ExtractGeometry : GeometryObject
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
}
