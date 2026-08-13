namespace RoadRegistry.RoadSegment.ValueObjects;

using RoadRegistry.ValueObjects;

// The start/end road nodes a segment is knotted into the network with. Both are null for a segment
// that is not realized: a planned segment carries no road nodes at all.
public sealed record RoadSegmentNodeIds
{
    public RoadNodeId? Start { get; init; }
    public RoadNodeId? End { get; init; }
}
