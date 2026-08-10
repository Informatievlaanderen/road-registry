namespace RoadRegistry.RoadSegment.Changes;

using ScopedRoadNetwork;

// 'Markeer een gepland wegsegment als gerealiseerd'. The action carries nothing but the road segment it applies to:
// the geometry is the one already on record, and everything else it does follows from the network around it.
public sealed record RealizeRoadSegmentChange : IRoadNetworkChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
}
