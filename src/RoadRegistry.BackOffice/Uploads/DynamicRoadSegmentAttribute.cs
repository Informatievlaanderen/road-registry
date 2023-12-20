namespace RoadRegistry.BackOffice.Uploads;

using System;

public abstract class DynamicRoadSegmentAttribute
{
    protected DynamicRoadSegmentAttribute(
        AttributeId temporaryId,
        RoadSegmentPosition from,
        RoadSegmentPosition to
    )
    {
        TemporaryId = temporaryId;
        From = from;
        To = to;
    }

    public RoadSegmentPosition From { get; }
    public AttributeId TemporaryId { get; }
    public RoadSegmentPosition To { get; }
}
