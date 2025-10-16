namespace RoadRegistry.RoadNetwork.ValueObjects;

using BackOffice;

public class RoadSegmentWidthAttributeChange : RoadSegmentDynamicAttributeChange
{
    public RoadSegmentWidthAttributeChange(
        AttributeId id,
        RoadSegmentWidth width,
        RoadSegmentPosition from,
        RoadSegmentPosition to
    ) : base(id,  from, to)
    {
        Width = width;
    }

    public RoadSegmentWidth Width { get; }
}
