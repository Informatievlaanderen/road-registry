namespace RoadRegistry.RoadSegment.ValueObjects;

using BackOffice;

public class RoadSegmentLaneAttribute : RoadSegmentDynamicAttribute
{
    public RoadSegmentLaneAttribute(
        AttributeId id,
        RoadSegmentPosition from,
        RoadSegmentPosition to,
        RoadSegmentLaneCount count,
        RoadSegmentLaneDirection direction
    ) : base(id, from, to)
    {
        Count = count;
        Direction = direction;
    }

    public RoadSegmentLaneCount Count { get; }
    public RoadSegmentLaneDirection Direction { get; }
}
