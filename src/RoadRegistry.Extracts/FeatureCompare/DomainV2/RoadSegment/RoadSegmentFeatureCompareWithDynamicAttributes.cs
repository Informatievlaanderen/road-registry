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
            CarAccessForward = CarAccessForward == other.CarAccessForward && CarAccessBackward == other.CarAccessBackward ? null : CarAccessForward,
            CarAccessBackward = CarAccessForward == other.CarAccessForward && CarAccessBackward == other.CarAccessBackward ? null : CarAccessBackward,
            BikeAccessForward = BikeAccessForward == other.BikeAccessForward && BikeAccessBackward == other.BikeAccessBackward ? null : BikeAccessForward,
            BikeAccessBackward = BikeAccessForward == other.BikeAccessForward && BikeAccessBackward == other.BikeAccessBackward ? null : BikeAccessBackward,
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

        // The positions add up the lengths of the flat parts, but the segment's geometry is the merged line - with
        // vertices within a centimetre of each other collapsed and rounded to the centimetre - which can measure a
        // centimetre more or less. The trailing position is landed on the geometry's own length, otherwise the domain
        // rejects the segment with a ToPositionNotEqualToLength error.
        var roundedGeometry = geometry.RoundToCm();
        var segmentLength = roundedGeometry.Length;

        return new RoadSegmentFeatureCompareWithDynamicAttributes
        {
            RoadSegmentId = roadSegmentId,
            Geometry = roundedGeometry,
            Method = method,
            Status = status,
            AccessRestriction = CreateDynamicAttributeValues(segmentLength, flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, x.AccessRestriction))),
            Category = CreateDynamicAttributeValues(segmentLength, flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, x.Category))),
            MaintenanceAuthorityId = CreateDynamicAttributeValues(segmentLength, flatAttributes.SelectMany(x => new []
            {
                (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Links, x.LeftMaintenanceAuthorityId),
                (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Rechts, x.RightMaintenanceAuthorityId)
            })),
            Morphology = CreateDynamicAttributeValues(segmentLength, flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, x.Morphology))),
            StreetNameId = CreateDynamicAttributeValues(segmentLength, flatAttributes.SelectMany(x => new []
            {
                (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Links, x.LeftSideStreetNameId),
                (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Rechts, x.RightSideStreetNameId)
            })),
            SurfaceType = CreateDynamicAttributeValues(segmentLength, flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, x.SurfaceType))),
            CarAccessForward = CreateDynamicAttributeValues(segmentLength, flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, x.CarAccessForward))),
            CarAccessBackward = CreateDynamicAttributeValues(segmentLength, flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, x.CarAccessBackward))),
            BikeAccessForward = CreateDynamicAttributeValues(segmentLength, flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, x.BikeAccessForward))),
            BikeAccessBackward = CreateDynamicAttributeValues(segmentLength, flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, x.BikeAccessBackward))),
            PedestrianAccess = CreateDynamicAttributeValues(segmentLength, flatAttributes.Select(x => (coveragePerGeometry[x.Geometry], RoadSegmentAttributeSide.Beide, x.PedestrianAccess)))
        };
    }

    // Snapping the end vertices onto their road nodes moves them by up to a centimetre, which lengthens or shortens
    // the outermost stretches of the line. The positions are remapped onto the new vertices the same way the domain
    // does when a segment is realized onto its nodes, so the trailing position keeps matching the geometry.
    public RoadSegmentFeatureCompareWithDynamicAttributes WithSnappedGeometry(MultiLineString snappedGeometry)
    {
        var currentVertexPositions = Geometry.GetSingleLineString().CumulativeVertexPositions();
        var newVertexPositions = snappedGeometry.GetSingleLineString().CumulativeVertexPositions();

        return this with
        {
            Geometry = snappedGeometry,
            AccessRestriction = AccessRestriction?.RemapTo(currentVertexPositions, newVertexPositions),
            Category = Category?.RemapTo(currentVertexPositions, newVertexPositions),
            MaintenanceAuthorityId = MaintenanceAuthorityId?.RemapTo(currentVertexPositions, newVertexPositions),
            Morphology = Morphology?.RemapTo(currentVertexPositions, newVertexPositions),
            StreetNameId = StreetNameId?.RemapTo(currentVertexPositions, newVertexPositions),
            SurfaceType = SurfaceType?.RemapTo(currentVertexPositions, newVertexPositions),
            CarAccessForward = CarAccessForward?.RemapTo(currentVertexPositions, newVertexPositions),
            CarAccessBackward = CarAccessBackward?.RemapTo(currentVertexPositions, newVertexPositions),
            BikeAccessForward = BikeAccessForward?.RemapTo(currentVertexPositions, newVertexPositions),
            BikeAccessBackward = BikeAccessBackward?.RemapTo(currentVertexPositions, newVertexPositions),
            PedestrianAccess = PedestrianAccess?.RemapTo(currentVertexPositions, newVertexPositions)
        };
    }

    private static RoadSegmentDynamicAttributeValues<T> CreateDynamicAttributeValues<T>(
        double segmentLength,
        IEnumerable<(RoadSegmentPositionCoverage Coverage, RoadSegmentAttributeSide Side, T Value)> attributeValues)
        where T : notnull
    {
        return new RoadSegmentDynamicAttributeValues<T>(attributeValues).WithTrailingCoverageSnappedTo(segmentLength);
    }
}
