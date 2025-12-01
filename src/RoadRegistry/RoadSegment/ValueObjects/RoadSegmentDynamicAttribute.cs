namespace RoadRegistry.RoadSegment.ValueObjects;

using RoadRegistry.ValueObjects;

public abstract class RoadSegmentDynamicAttribute : IRoadSegmentDynamicAttribute
{
    protected RoadSegmentDynamicAttribute(
        AttributeId id,
        RoadSegmentPosition from,
        RoadSegmentPosition to
    )
    {
        Id = id;
        From = from;
        To = to;
    }

    public AttributeId Id { get; }
    public RoadSegmentPosition From { get; }
    public RoadSegmentPosition To { get; }
}
