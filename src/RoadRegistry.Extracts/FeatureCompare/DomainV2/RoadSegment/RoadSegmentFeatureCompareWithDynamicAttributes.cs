namespace RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;

using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadSegment.ValueObjects;

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
    // Traffic types, the way the road network itself records them. A delivery states them per direction as a pair of
    // booleans, either of which may be unknown ('-8'), and 'niet gekend' is where such a pair lands.
    public RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>? CarTrafficDirection { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>? BikeTrafficDirection { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>? PedestrianTrafficDirection { get; init; }

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
            CarTrafficDirection = CarTrafficDirection == other.CarTrafficDirection ? null : CarTrafficDirection,
            BikeTrafficDirection = BikeTrafficDirection == other.BikeTrafficDirection ? null : BikeTrafficDirection,
            PedestrianTrafficDirection = PedestrianTrafficDirection == other.PedestrianTrafficDirection ? null : PedestrianTrafficDirection
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
            Geometry = geometry.RoundToCm(),
            Method = method,
            Status = status,
            AccessRestriction = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, x.AccessRestriction))),
            Category = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, x.Category))),
            MaintenanceAuthorityId = CreateDynamicAttributeValues(flatAttributes.SelectMany(x => new []
            {
                (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Links, x.LeftMaintenanceAuthorityId),
                (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Rechts, x.RightMaintenanceAuthorityId)
            })),
            Morphology = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, x.Morphology))),
            StreetNameId = CreateDynamicAttributeValues(flatAttributes.SelectMany(x => new []
            {
                (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Links, x.LeftSideStreetNameId),
                (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Rechts, x.RightSideStreetNameId)
            })),
            SurfaceType = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, x.SurfaceType))),
            CarTrafficDirection = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, RoadSegmentTrafficDirection.FromAccess(x.CarAccessForward, x.CarAccessBackward)))),
            BikeTrafficDirection = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, RoadSegmentTrafficDirection.FromAccess(x.BikeAccessForward, x.BikeAccessBackward)))),
            PedestrianTrafficDirection = CreateDynamicAttributeValues(flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, RoadSegmentPedestrianTrafficDirection.FromAccess(x.PedestrianAccess))))
        };
    }

    private static RoadSegmentDynamicAttributeValues<T> CreateDynamicAttributeValues<T>(
        IEnumerable<(RoadSegmentPositionCoverage Coverage, RoadSegmentAttributeSide Side, T Value)> attributeValues)
        where T : notnull
    {
        return new RoadSegmentDynamicAttributeValues<T>(attributeValues);
    }
}
