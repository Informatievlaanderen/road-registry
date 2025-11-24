namespace RoadRegistry.RoadNode.Changes;

using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadRegistry.BackOffice;

public sealed record AddRoadNodeChange : IRoadNetworkChange
{
    public required RoadNodeId TemporaryId { get; init; }
    public required RoadNodeId OriginalId { get; init; }
    public required Point Geometry { get; init; }
    public required RoadNodeType Type { get; init; }
}
