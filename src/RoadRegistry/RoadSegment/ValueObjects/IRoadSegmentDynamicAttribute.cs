namespace RoadRegistry.RoadSegment.ValueObjects;

using RoadRegistry.ValueObjects;

public interface IRoadSegmentDynamicAttribute
{
    AttributeId Id { get; }
    RoadSegmentPosition From { get; }
    RoadSegmentPosition To { get; }
}
