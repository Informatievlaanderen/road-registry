namespace RoadRegistry.RoadSegment.ValueObjects;

using BackOffice;

public class RoadSegmentSurfaceAttribute : RoadSegmentDynamicAttribute
{
    public RoadSegmentSurfaceAttribute(
        AttributeId id,
        RoadSegmentPosition from,
        RoadSegmentPosition to,
        RoadSegmentSurfaceType type
    ) : base(id, from, to)
    {
        Type = type;
    }

    public RoadSegmentSurfaceType Type { get; }
}
