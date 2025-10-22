namespace RoadRegistry.RoadSegment.ValueObjects;

using BackOffice;

public class RoadSegmentWidthAttribute : RoadSegmentDynamicAttribute
{
    public RoadSegmentWidthAttribute(
        AttributeId id,
        RoadSegmentPosition from,
        RoadSegmentPosition to,
        RoadSegmentWidth width
    ) : base(id, from, to)
    {
        Width = width;
    }

    public RoadSegmentWidth Width { get; }
}
