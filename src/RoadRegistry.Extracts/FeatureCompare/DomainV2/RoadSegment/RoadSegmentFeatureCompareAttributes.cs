namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using NetTopologySuite.Geometries;

public record RoadSegmentFeatureCompareAttributes
{
    public RoadSegmentId TempId { get; init; }
    public RoadSegmentId? RoadSegmentId { get; init; }
    public MultiLineString? Geometry { get; init; }
    public RoadNodeId? StartNodeId { get; init; }
    public RoadNodeId? EndNodeId { get; init; }
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
    public bool? CarAccessForward { get; init; }
    public bool? CarAccessBackward { get; init; }
    public bool? BikeAccessForward { get; init; }
    public bool? BikeAccessBackward { get; init; }
    public bool? PedestrianAccess { get; init; }

    public RoadSegmentFeatureCompareAttributes OnlyChangedAttributes(RoadSegmentFeatureCompareAttributes other, MultiLineString extractGeometry, bool alwaysIncludeNodeIds)
    {
        return new RoadSegmentFeatureCompareAttributes
        {
            TempId = TempId,
            Geometry = Geometry.EqualsExact(other.Geometry) ? extractGeometry : Geometry,
            StartNodeId = StartNodeId != other.StartNodeId || alwaysIncludeNodeIds ? StartNodeId : null,
            EndNodeId = EndNodeId != other.EndNodeId || alwaysIncludeNodeIds ? EndNodeId : null,
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
            CarAccessForward = CarAccessForward == other.CarAccessForward ? null : CarAccessForward,
            CarAccessBackward = CarAccessBackward == other.CarAccessBackward ? null : CarAccessBackward,
            BikeAccessForward = BikeAccessForward == other.BikeAccessForward ? null : BikeAccessForward,
            BikeAccessBackward = BikeAccessBackward == other.BikeAccessBackward ? null : BikeAccessBackward,
            PedestrianAccess = PedestrianAccess == other.PedestrianAccess ? null : PedestrianAccess
        };
    }
}
