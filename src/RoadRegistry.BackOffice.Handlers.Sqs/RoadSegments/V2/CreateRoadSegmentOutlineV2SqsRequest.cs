namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments.V2;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.RoadSegment.ValueObjects;

[BlobRequest]
public sealed class CreateRoadSegmentOutlineV2SqsRequest : SqsRequest
{
    public required RoadSegmentGeometry Geometry { get; init; }
    public required RoadSegmentStatusV2 Status { get; init; }
    public required ChangeRoadSegmentMorphologyAttributeValue[] Morphology { get; init; }
    public required ChangeRoadSegmentSurfaceTypeAttributeValue[] SurfaceType { get; init; }
    public required ChangeRoadSegmentAccessRestrictionAttributeValue[] AccessRestriction { get; init; }
    public required ChangeRoadSegmentStreetNameIdAttributeValue[] StreetNameId { get; init; }
    public required ChangeRoadSegmentMaintenanceAuthorityIdAttributeValue[] MaintenanceAuthorityId { get; init; }
    public required ChangeRoadSegmentCategoryAttributeValue[] Category { get; init; }
    public required ChangeRoadSegmentCarTrafficDirectionAttributeValue[] CarTrafficDirection { get; init; }
    public required ChangeRoadSegmentBikeTrafficDirectionAttributeValue[] BikeTrafficDirection { get; init; }
    public required ChangeRoadSegmentPedestrianTrafficDirectionAttributeValue[] PedestrianTrafficDirection { get; init; }
}

public class ChangeRoadSegmentStreetNameIdAttributeValue
{
    public required RoadSegmentAttributeSide Side { get; init; }
    public required RoadSegmentPositionV2 FromPosition { get; init; }
    public required RoadSegmentPositionV2 ToPosition { get; init; }
    public required StreetNameLocalId StreetNameId { get; init; }
}

public class ChangeRoadSegmentMaintenanceAuthorityIdAttributeValue
{
    public required RoadSegmentAttributeSide Side { get; init; }
    public required RoadSegmentPositionV2 FromPosition { get; init; }
    public required RoadSegmentPositionV2 ToPosition { get; init; }
    public required OrganizationId MaintenanceAuthorityId { get; init; }
}

public class ChangeRoadSegmentMorphologyAttributeValue
{
    public required RoadSegmentPositionV2 FromPosition { get; init; }
    public required RoadSegmentPositionV2 ToPosition { get; init; }
    public required RoadSegmentMorphologyV2 Morphology { get; init; }
}

public class ChangeRoadSegmentAccessRestrictionAttributeValue
{
    public required RoadSegmentPositionV2 FromPosition { get; init; }
    public required RoadSegmentPositionV2 ToPosition { get; init; }
    public required RoadSegmentAccessRestrictionV2 AccessRestriction { get; init; }
}

public class ChangeRoadSegmentCategoryAttributeValue
{
    public required RoadSegmentPositionV2 FromPosition { get; init; }
    public required RoadSegmentPositionV2 ToPosition { get; init; }
    public required RoadSegmentCategoryV2 Category { get; init; }
}

public class ChangeRoadSegmentSurfaceTypeAttributeValue
{
    public required RoadSegmentPositionV2 FromPosition { get; init; }
    public required RoadSegmentPositionV2 ToPosition { get; init; }
    public required RoadSegmentSurfaceTypeV2 SurfaceType { get; init; }
}

public class ChangeRoadSegmentCarTrafficDirectionAttributeValue
{
    public required RoadSegmentPositionV2 FromPosition { get; init; }
    public required RoadSegmentPositionV2 ToPosition { get; init; }
    public required RoadSegmentTrafficDirection TrafficDirection { get; init; }
}

public class ChangeRoadSegmentBikeTrafficDirectionAttributeValue
{
    public required RoadSegmentPositionV2 FromPosition { get; init; }
    public required RoadSegmentPositionV2 ToPosition { get; init; }
    public required RoadSegmentTrafficDirection TrafficDirection { get; init; }
}

public class ChangeRoadSegmentPedestrianTrafficDirectionAttributeValue
{
    public required RoadSegmentPositionV2 FromPosition { get; init; }
    public required RoadSegmentPositionV2 ToPosition { get; init; }
    public required RoadSegmentPedestrianTrafficDirection TrafficDirection { get; init; }
}
