namespace RoadRegistry.BackOffice.FeatureCompare.V3.RoadSegment;

using NetTopologySuite.Geometries;
using RoadRegistry.RoadSegment.ValueObjects;

public record RoadSegmentFeatureCompareAttributes
{
    public RoadSegmentId Id { get; init; }
    public MultiLineString Geometry { get; init; }
    public RoadNodeId? StartNodeId { get; init; }
    public RoadNodeId? EndNodeId { get; init; }
    public RoadSegmentGeometryDrawMethod? Method { get; init; }
    public RoadSegmentAccessRestriction? AccessRestriction { get; init; }
    public RoadSegmentCategory? Category { get; init; }
    public OrganizationId? MaintenanceAuthority { get; init; }
    public RoadSegmentMorphology? Morphology { get; init; }
    public RoadSegmentStatus? Status { get; init; }
    public StreetNameLocalId? LeftSideStreetNameId { get; init; }
    public StreetNameLocalId? RightSideStreetNameId { get; init; }

    public RoadSegmentFeatureCompareAttributes OnlyChangedAttributes(RoadSegmentFeatureCompareAttributes other, MultiLineString extractGeometry, bool alwaysIncludeNodeIds)
    {
        return new RoadSegmentFeatureCompareAttributes
        {
            Id = Id,
            Geometry = Geometry!.EqualsExact(other.Geometry) ? extractGeometry : Geometry,
            StartNodeId = StartNodeId != other.StartNodeId || alwaysIncludeNodeIds ? StartNodeId : null,
            EndNodeId = EndNodeId != other.EndNodeId || alwaysIncludeNodeIds ? EndNodeId : null,
            Method = Method == other.Method ? null : Method,
            AccessRestriction = AccessRestriction == other.AccessRestriction ? null : AccessRestriction,
            Category = Category == other.Category ? null : Category,
            MaintenanceAuthority = MaintenanceAuthority == other.MaintenanceAuthority ? null : MaintenanceAuthority,
            Morphology = Morphology == other.Morphology ? null : Morphology,
            Status = Status == other.Status ? null : Status,
            LeftSideStreetNameId = LeftSideStreetNameId == other.LeftSideStreetNameId && RightSideStreetNameId == other.RightSideStreetNameId ? null : LeftSideStreetNameId,
            RightSideStreetNameId = LeftSideStreetNameId == other.LeftSideStreetNameId && RightSideStreetNameId == other.RightSideStreetNameId ? null : RightSideStreetNameId
        };
    }
}
