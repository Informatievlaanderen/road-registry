namespace RoadRegistry.RoadNetwork.Events;

using ValueObjects;

public record RoadNetworkChanged
{
    public required GeometryObject ScopeGeometry { get; init; }
}
