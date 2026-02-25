namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment.ValueObjects;

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
        RoadSegmentStatusV2 status,
        IReadOnlyCollection<RoadSegmentFeatureCompareWithFlatAttributes> flatAttributes)
    {
        var fromPosition = 0.0;
        var toPosition = 0.0;

        var coveragePerGeometry = flatAttributes
            .ToDictionary(x => x.Geometry, x =>
            {
                fromPosition = toPosition;
                toPosition += x.Geometry.Length;
                return new RoadSegmentPositionCoverage(new RoadSegmentPositionV2(fromPosition), new RoadSegmentPositionV2(toPosition));
            });

        return new RoadSegmentFeatureCompareWithDynamicAttributes
        {
            RoadSegmentId = roadSegmentId,
            Geometry = geometry,
            Method = method,
            Status = status,
            AccessRestriction = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Both, x.AccessRestriction))),
            Category = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Both, x.Category))),
            MaintenanceAuthorityId = CreateDynamicAttributeValues(flatAttributes.SelectMany(x => new []
            {
                (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Left, x.LeftMaintenanceAuthorityId),
                (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Right, x.RightMaintenanceAuthorityId)
            })),
            Morphology = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Both, x.Morphology))),
            StreetNameId = CreateDynamicAttributeValues(flatAttributes.SelectMany(x => new []
            {
                (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Left, x.LeftSideStreetNameId),
                (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Right, x.RightSideStreetNameId)
            })),
            SurfaceType = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Both, x.SurfaceType))),
            CarAccessForward = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Both, x.CarAccessForward))),
            CarAccessBackward = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Both, x.CarAccessBackward))),
            BikeAccessForward = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Both, x.BikeAccessForward))),
            BikeAccessBackward = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Both, x.BikeAccessBackward))),
            PedestrianAccess = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Both, x.PedestrianAccess)))
        };
    }

    private static RoadSegmentDynamicAttributeValues<T> CreateDynamicAttributeValues<T>(
        IEnumerable<(RoadSegmentPositionCoverage Coverage, RoadSegmentAttributeSide Side, T Value)> attributeValues)
        where T : notnull
    {
        return new RoadSegmentDynamicAttributeValues<T>(attributeValues);
    }
}
