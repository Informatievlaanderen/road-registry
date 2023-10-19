namespace RoadRegistry.Tests.BackOffice.Scenarios;

using RoadRegistry.BackOffice.Messages;

internal static class RoadNetworkExtensions
{
    public static RoadSegmentLaneAttributes[] ToRoadSegmentLaneAttributes(this RequestedRoadSegmentLaneAttribute[] lanes, int? startAttributeId = null, int asOfGeometryVersion = 1)
    {
        return lanes
            .Select((lane, index) => new RoadSegmentLaneAttributes
            {
                AttributeId = startAttributeId is not null ? startAttributeId.Value + index + 1 : lane.AttributeId,
                Direction = lane.Direction,
                Count = lane.Count,
                FromPosition = lane.FromPosition,
                ToPosition = lane.ToPosition,
                AsOfGeometryVersion = asOfGeometryVersion
            })
            .ToArray();
    }

    public static RoadSegmentSurfaceAttributes[] ToRoadSegmentSurfaceAttributes(this RequestedRoadSegmentSurfaceAttribute[] surfaces, int? startAttributeId = null, int asOfGeometryVersion = 1)
    {
        return surfaces
        .Select((surface, index) => new RoadSegmentSurfaceAttributes
        {
                AttributeId = startAttributeId is not null ? startAttributeId.Value + index + 1 : surface.AttributeId,
                Type = surface.Type,
                FromPosition = surface.FromPosition,
                ToPosition = surface.ToPosition,
                AsOfGeometryVersion = asOfGeometryVersion
            })
            .ToArray();
    }

    public static RoadSegmentWidthAttributes[] ToRoadSegmentWidthAttributes(this RequestedRoadSegmentWidthAttribute[] widths, int? startAttributeId = null, int asOfGeometryVersion = 1)
    {
        return widths
            .Select((width, index) => new RoadSegmentWidthAttributes
            {
                AttributeId = startAttributeId is not null ? startAttributeId.Value + index + 1 : width.AttributeId,
                Width = width.Width,
                FromPosition = width.FromPosition,
                ToPosition = width.ToPosition,
                AsOfGeometryVersion = asOfGeometryVersion
            })
            .ToArray();
    }
}
