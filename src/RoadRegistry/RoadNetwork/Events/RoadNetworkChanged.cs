namespace RoadRegistry.RoadNetwork.Events;

using RoadRegistry.ValueObjects;

public record RoadNetworkChanged
{
    public GeometryObject? ScopeGeometry { get; init; }
    public DownloadId? DownloadId { get; init; }

    public RoadNetworkChanged()
    {
    }
    protected RoadNetworkChanged(RoadNetworkChanged other) // Needed for Marten
    {
    }
}
