namespace RoadRegistry.RoadNode.Changes;

using NetTopologySuite.Geometries;
using ScopedRoadNetwork;

public sealed record AddRoadNodeChange : IRoadNetworkChange
{
    public required RoadNodeId TemporaryId { get; init; }
    public RoadNodeId? OriginalId { get; init; }
    public required RoadNodeGeometry Geometry { get; init; }
    public required RoadNodeType Type { get; init; }
}
