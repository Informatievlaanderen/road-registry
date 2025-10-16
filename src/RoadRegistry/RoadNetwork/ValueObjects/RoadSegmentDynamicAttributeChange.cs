namespace RoadRegistry.RoadNetwork.ValueObjects;

using BackOffice;

public abstract class RoadSegmentDynamicAttributeChange
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
