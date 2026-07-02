namespace RoadRegistry.Projections.Tests.Projections.ReadProjections.RoadSegment;

using RoadRegistry.RoadSegment.ValueObjects;

public static class StreetNameAttributeBuilder
{
    public static RoadSegmentDynamicAttributeValues<StreetNameLocalId> Single(RoadSegmentGeometry geometry, StreetNameLocalId value)
    {
        return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(value, geometry);
    }

    public static RoadSegmentDynamicAttributeValues<StreetNameLocalId> LeftRight(RoadSegmentGeometry geometry, StreetNameLocalId left, StreetNameLocalId right)
    {
        var to = new RoadSegmentPositionV2(geometry.Value.Length);
        return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
            .Add(RoadSegmentPositionV2.Zero, to, RoadSegmentAttributeSide.Left, left)
            .Add(RoadSegmentPositionV2.Zero, to, RoadSegmentAttributeSide.Right, right);
    }
}
