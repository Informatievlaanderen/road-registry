namespace RoadRegistry.BackOffice.Uploads;

public class RoadSegmentSurfaceAttribute : DynamicRoadSegmentAttribute
{
    public RoadSegmentSurfaceAttribute(
        AttributeId temporaryId,
        RoadSegmentSurfaceType type,
        RoadSegmentPosition from,
        RoadSegmentPosition to
    ) : base(temporaryId, from, to)
    {
        Type = type;
    }

    public RoadSegmentSurfaceType Type { get; }
}
