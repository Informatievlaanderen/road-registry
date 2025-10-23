namespace RoadRegistry.RoadSegment.ValueObjects;

using BackOffice;

public interface IRoadSegmentDynamicAttribute
{
    AttributeId Id { get; }
    RoadSegmentPosition From { get; }
    RoadSegmentPosition To { get; }
}
