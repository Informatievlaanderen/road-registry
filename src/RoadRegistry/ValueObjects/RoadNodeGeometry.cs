namespace RoadRegistry.ValueObjects;

using Extensions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public record RoadNodeGeometry : GeometryObject
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

    public static RoadNodeGeometry Create(Geometry geometry)
    {
        return new RoadNodeGeometry(geometry);
    }
}
