namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment.ValueObjects;

public record RoadSegmentFeatureCompareAttributes
{
    public RoadSegmentId TempId { get; init; }
    public RoadSegmentId? RoadSegmentId { get; init; }
    public required MultiLineString Geometry { get; init; }
    public RoadSegmentGeometryDrawMethodV2? Method { get; init; }
    public RoadSegmentAccessRestrictionV2? AccessRestriction { get; init; }
    public RoadSegmentCategoryV2? Category { get; init; }
    public OrganizationId? LeftMaintenanceAuthorityId { get; init; }
    public OrganizationId? RightMaintenanceAuthorityId { get; init; }
    public RoadSegmentMorphologyV2? Morphology { get; init; }
    public RoadSegmentStatusV2? Status { get; init; }
    public StreetNameLocalId? LeftSideStreetNameId { get; init; }
    public StreetNameLocalId? RightSideStreetNameId { get; init; }
    public RoadSegmentSurfaceTypeV2? SurfaceType { get; init; }
    public VehicleAccess? CarAccess { get; init; }
    public VehicleAccess? BikeAccess { get; init; }
    public bool? PedestrianAccess { get; init; }

    public RoadSegmentFeatureCompareAttributes OnlyChangedAttributes(RoadSegmentFeatureCompareAttributes other, MultiLineString extractGeometry)
    {
        return new RoadSegmentFeatureCompareAttributes
        {
            TempId = TempId,
            Geometry = Geometry.EqualsExact(other.Geometry) ? extractGeometry : Geometry,
            Method = Method == other.Method ? null : Method,
            AccessRestriction = AccessRestriction == other.AccessRestriction ? null : AccessRestriction,
            Category = Category == other.Category ? null : Category,
            LeftMaintenanceAuthorityId = LeftMaintenanceAuthorityId == other.LeftMaintenanceAuthorityId ? null : LeftMaintenanceAuthorityId,
            RightMaintenanceAuthorityId = RightMaintenanceAuthorityId == other.RightMaintenanceAuthorityId ? null : RightMaintenanceAuthorityId,
            Morphology = Morphology == other.Morphology ? null : Morphology,
            Status = Status == other.Status ? null : Status,
            LeftSideStreetNameId = LeftSideStreetNameId == other.LeftSideStreetNameId && RightSideStreetNameId == other.RightSideStreetNameId ? null : LeftSideStreetNameId,
            RightSideStreetNameId = LeftSideStreetNameId == other.LeftSideStreetNameId && RightSideStreetNameId == other.RightSideStreetNameId ? null : RightSideStreetNameId,
            SurfaceType = SurfaceType == other.SurfaceType ? null : SurfaceType,
            CarAccess = CarAccess == other.CarAccess ? null : CarAccess,
            BikeAccess = BikeAccess == other.BikeAccess ? null : BikeAccess,
            PedestrianAccess = PedestrianAccess == other.PedestrianAccess ? null : PedestrianAccess
        };
    }
}
