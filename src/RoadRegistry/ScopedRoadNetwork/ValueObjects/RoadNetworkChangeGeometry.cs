namespace RoadRegistry.ScopedRoadNetwork.ValueObjects;

using NetTopologySuite.Geometries;

public record RoadNetworkChangeGeometry : GeometryObject
{
    public RoadNetworkChangeGeometry(int srid, string wkt) : base(srid, wkt)
    {
    }

    private RoadNetworkChangeGeometry(Geometry geometry) : base(geometry)
    {
    }

    public static RoadNetworkChangeGeometry Create(Geometry geometry)
    {
        return new RoadNetworkChangeGeometry(geometry);
    }
}
