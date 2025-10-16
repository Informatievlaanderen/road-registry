namespace RoadRegistry.RoadNetwork.ValueObjects;

using BackOffice;

public class RoadSegmentLaneAttributeChange : RoadSegmentDynamicAttributeChange
{
    public RoadSegmentLaneAttributeChange(
        AttributeId id,
        RoadSegmentLaneCount count,
        RoadSegmentLaneDirection direction,
        RoadSegmentPosition from,
        RoadSegmentPosition to
    ) : base(id, from, to)
    {
        Count = count;
        Direction = direction;
    }

    public RoadSegmentLaneCount Count { get; }
    public RoadSegmentLaneDirection Direction { get; }
}
