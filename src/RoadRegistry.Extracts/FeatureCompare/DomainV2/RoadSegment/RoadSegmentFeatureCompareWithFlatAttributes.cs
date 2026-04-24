namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using NetTopologySuite.Geometries;

public record RoadSegmentFeatureCompareWithFlatAttributes
{
    public required RoadSegmentTempId TempId { get; init; }
    public required RoadSegmentId? RoadSegmentId { get; init; }
    public required MultiLineString Geometry { get; init; }
    public required RoadSegmentGeometryDrawMethodV2? Method { get; init; }
    public required RoadSegmentStatusV2 Status { get; init; }
    public required RoadSegmentAccessRestrictionV2 AccessRestriction { get; init; }
    public required RoadSegmentCategoryV2 Category { get; init; }
    public required OrganizationId LeftMaintenanceAuthorityId { get; init; }
    public required OrganizationId RightMaintenanceAuthorityId { get; init; }
    public required RoadSegmentMorphologyV2 Morphology { get; init; }
    public required StreetNameLocalId LeftSideStreetNameId { get; init; }
    public required StreetNameLocalId RightSideStreetNameId { get; init; }
    public required RoadSegmentSurfaceTypeV2 SurfaceType { get; init; }
    public required bool CarAccessForward { get; init; }
    public required bool CarAccessBackward { get; init; }
    public required bool BikeAccessForward { get; init; }
    public required bool BikeAccessBackward { get; init; }
    public required bool PedestrianAccess { get; init; }
}
