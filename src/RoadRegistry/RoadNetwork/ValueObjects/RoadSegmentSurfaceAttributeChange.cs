namespace RoadRegistry.RoadNetwork.ValueObjects;

using BackOffice;

public class RoadSegmentSurfaceAttributeChange : RoadSegmentDynamicAttributeChange
{
    public RoadSegmentSurfaceAttributeChange(
        AttributeId id,
        RoadSegmentSurfaceType type,
        RoadSegmentPosition from,
        RoadSegmentPosition to
    ) : base(id, from, to)
    {
        Type = type;
    }

    public RoadSegmentSurfaceType Type { get; }
}
