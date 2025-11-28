namespace RoadRegistry.BackOffice.Core;

using RoadRegistry.RoadNetwork.ValueObjects;

public class RoadSegmentSurfaceAttribute : DynamicRoadSegmentAttribute
{
    public RoadSegmentSurfaceAttribute(
        AttributeId id,
        AttributeId temporaryId,
        RoadSegmentSurfaceType type,
        RoadSegmentPosition from,
        RoadSegmentPosition to,
        GeometryVersion asOfGeometryVersion
    ) : base(id, temporaryId, from, to, asOfGeometryVersion)
    {
        Type = type;
    }

    public RoadSegmentSurfaceType Type { get; }
}
