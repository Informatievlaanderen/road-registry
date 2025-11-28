namespace RoadRegistry.BackOffice.Core;

using RoadRegistry.RoadNetwork.ValueObjects;

public class RoadSegmentWidthAttribute : DynamicRoadSegmentAttribute
{
    public RoadSegmentWidthAttribute(
        AttributeId id,
        AttributeId temporaryId,
        RoadSegmentWidth width,
        RoadSegmentPosition from,
        RoadSegmentPosition to,
        GeometryVersion asOfGeometryVersion
    ) : base(id, temporaryId, from, to, asOfGeometryVersion)
    {
        Width = width;
    }

    public RoadSegmentWidth Width { get; }
}
