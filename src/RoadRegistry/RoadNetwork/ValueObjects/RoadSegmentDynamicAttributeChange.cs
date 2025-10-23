namespace RoadRegistry.RoadNetwork.ValueObjects;

using BackOffice;
using RoadSegment.ValueObjects;

public abstract class RoadSegmentDynamicAttributeChange : IRoadSegmentDynamicAttribute
{
    protected RoadSegmentDynamicAttributeChange(
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
