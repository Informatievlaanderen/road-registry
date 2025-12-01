namespace RoadRegistry.BackOffice.Uploads;

public class RoadSegmentLaneAttribute : DynamicRoadSegmentAttribute
{
    public RoadSegmentLaneAttribute(
        AttributeId temporaryId,
        RoadSegmentLaneCount count,
        RoadSegmentLaneDirection direction,
        RoadSegmentPosition from,
        RoadSegmentPosition to
    ) : base(temporaryId, from, to)
    {
        Count = count;
        Direction = direction;
    }

    public RoadSegmentLaneCount Count { get; }
    public RoadSegmentLaneDirection Direction { get; }
}
