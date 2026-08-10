namespace RoadRegistry.RoadSegment.Changes;

// 'Markeer een gepland wegsegment als gerealiseerd'. The action carries nothing but the road segment it applies to:
// the geometry is the one already on record, and everything else it does follows from the network around it.
//
// Deliberately not an IRoadNetworkChange: like a split or a geometry change, this operation performs its own very
// specific mutations (snapping onto road nodes, adding the ones that are missing) rather than being fed through the
// generic change pipeline.
public sealed record RealizeRoadSegmentChange
{
    public required RoadSegmentId RoadSegmentId { get; init; }
}
