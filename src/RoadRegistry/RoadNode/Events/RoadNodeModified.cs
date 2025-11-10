namespace RoadRegistry.RoadNode.Events;

using BackOffice;
using RoadNetwork.ValueObjects;

public class RoadNodeModified
{
    public required RoadNodeId Id { get; init; }
    public required GeometryObject Geometry { get; init; }
    public required RoadNodeType Type { get; init; }
}
