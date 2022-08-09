namespace RoadRegistry.BackOffice.Uploads;

public class RoadSegmentWidthAttribute : DynamicRoadSegmentAttribute
{
    public RoadSegmentWidthAttribute(
        AttributeId temporaryId,
        RoadSegmentWidth width,
        RoadSegmentPosition from,
        RoadSegmentPosition to
    ) : base(temporaryId, from, to)
    {
        Width = width;
    }

    public RoadSegmentWidth Width { get; }
}
