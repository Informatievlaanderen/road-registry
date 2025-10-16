namespace RoadRegistry.BackOffice.Core;

public class RoadSegmentLaneAttribute : DynamicRoadSegmentAttribute
{
    public RoadSegmentLaneAttribute(
        AttributeId id,
        AttributeId temporaryId,
        RoadSegmentLaneCount count,
        RoadSegmentLaneDirection direction,
        RoadSegmentPosition from,
        RoadSegmentPosition to,
        GeometryVersion asOfGeometryVersion
    ) : base(id, temporaryId, from, to, asOfGeometryVersion)
    {
        Count = count;
        Direction = direction;
    }

    public RoadSegmentLaneCount Count { get; }
    public RoadSegmentLaneDirection Direction { get; }
}