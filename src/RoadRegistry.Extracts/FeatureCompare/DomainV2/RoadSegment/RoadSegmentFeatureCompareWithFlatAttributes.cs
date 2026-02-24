namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment.ValueObjects;

public record RoadSegmentFeatureCompareWithFlatAttributes
{
    public RoadSegmentTempId TempId { get; init; }
    public RoadSegmentId? RoadSegmentId { get; init; }
    public required MultiLineString Geometry { get; init; }
    public RoadSegmentGeometryDrawMethodV2? Method { get; init; }
    public RoadSegmentStatusV2? Status { get; init; }
    public RoadSegmentAccessRestrictionV2? AccessRestriction { get; init; }
    public RoadSegmentCategoryV2? Category { get; init; }
    public OrganizationId? LeftMaintenanceAuthorityId { get; init; }
    public OrganizationId? RightMaintenanceAuthorityId { get; init; }
    public RoadSegmentMorphologyV2? Morphology { get; init; }
    public StreetNameLocalId? LeftSideStreetNameId { get; init; }
    public StreetNameLocalId? RightSideStreetNameId { get; init; }
    public RoadSegmentSurfaceTypeV2? SurfaceType { get; init; }
    public bool? CarAccessForward { get; init; }
    public bool? CarAccessBackward { get; init; }
    public bool? BikeAccessForward { get; init; }
    public bool? BikeAccessBackward { get; init; }
    public bool? PedestrianAccess { get; init; }
}

public record RoadSegmentFeatureCompareWithDynamicAttributes
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public required MultiLineString Geometry { get; init; }
    public RoadSegmentGeometryDrawMethodV2? Method { get; init; }
    public RoadSegmentStatusV2? Status { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>? AccessRestriction { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>? Category { get; init; }
    public RoadSegmentDynamicAttributeValues<OrganizationId>? MaintenanceAuthorityId { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>? Morphology { get; init; }
    public RoadSegmentDynamicAttributeValues<StreetNameLocalId>? StreetNameId { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>? SurfaceType { get; init; }
    public RoadSegmentDynamicAttributeValues<bool>? CarAccessForward { get; init; }
    public RoadSegmentDynamicAttributeValues<bool>? CarAccessBackward { get; init; }
    public RoadSegmentDynamicAttributeValues<bool>? BikeAccessForward { get; init; }
    public RoadSegmentDynamicAttributeValues<bool>? BikeAccessBackward { get; init; }
    public RoadSegmentDynamicAttributeValues<bool>? PedestrianAccess { get; init; }

    public RoadSegmentFeatureCompareWithDynamicAttributes OnlyChangedAttributes(RoadSegmentFeatureCompareWithDynamicAttributes other, MultiLineString extractGeometry)
    {
        return new RoadSegmentFeatureCompareWithDynamicAttributes
        {
            RoadSegmentId = RoadSegmentId,
            Geometry = Geometry.EqualsExact(other.Geometry) ? extractGeometry : Geometry,
            Method = Method == other.Method ? null : Method,
            Status = Status == other.Status ? null : Status,
            AccessRestriction = AccessRestriction == other.AccessRestriction ? null : AccessRestriction,
            Category = Category == other.Category ? null : Category,
            MaintenanceAuthorityId = MaintenanceAuthorityId == other.MaintenanceAuthorityId ? null : MaintenanceAuthorityId,
            Morphology = Morphology == other.Morphology ? null : Morphology,
            StreetNameId = StreetNameId == other.StreetNameId ? null : StreetNameId,
            SurfaceType = SurfaceType == other.SurfaceType ? null : SurfaceType,
            CarAccessForward = CarAccessForward == other.CarAccessForward ? null : CarAccessForward,
            CarAccessBackward = CarAccessBackward == other.CarAccessBackward ? null : CarAccessBackward,
            BikeAccessForward = BikeAccessForward == other.BikeAccessForward ? null : BikeAccessForward,
            BikeAccessBackward = BikeAccessBackward == other.BikeAccessBackward ? null : BikeAccessBackward,
            PedestrianAccess = PedestrianAccess == other.PedestrianAccess ? null : PedestrianAccess
        };
    }

    public static RoadSegmentFeatureCompareWithDynamicAttributes Build(
        RoadSegmentId roadSegmentId,
        MultiLineString geometry,
        RoadSegmentGeometryDrawMethodV2 method,
        IReadOnlyCollection<RoadSegmentFeatureCompareWithFlatAttributes> flatAttributes)
    {
        //TODO-pr merge flat into dynamic attributes
        throw new NotImplementedException();
    }
}
