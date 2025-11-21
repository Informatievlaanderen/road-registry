namespace RoadRegistry.RoadNetwork.Events;

using BackOffice;
using ValueObjects;

public record RoadNetworkChanged
{
    public required GeometryObject ScopeGeometry { get; init; }
    public DownloadId? DownloadId { get; init; }
}
