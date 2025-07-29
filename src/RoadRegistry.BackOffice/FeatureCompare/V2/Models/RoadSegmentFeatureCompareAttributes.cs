namespace RoadRegistry.BackOffice.FeatureCompare.V2.Models;

using NetTopologySuite.Geometries;

public record RoadSegmentFeatureCompareAttributes
{
    public RoadSegmentId Id { get; init; }
    public RoadSegmentGeometryDrawMethod Method { get; init; }
    public MultiLineString? Geometry { get; init; }
    public RoadNodeId? StartNodeId { get; init; }
    public RoadNodeId? EndNodeId { get; init; }
    public RoadSegmentAccessRestriction? AccessRestriction { get; init; }
    public RoadSegmentCategory? Category { get; init; }
    public OrganizationId? MaintenanceAuthority { get; init; }
    public RoadSegmentMorphology? Morphology { get; init; }
    public RoadSegmentStatus? Status { get; init; }
    public StreetNameLocalId? LeftSideStreetNameId { get; init; }
    public StreetNameLocalId? RightSideStreetNameId { get; init; }

    public RoadSegmentFeatureCompareAttributes OnlyChangedAttributes(RoadSegmentFeatureCompareAttributes other)
    {
        return new RoadSegmentFeatureCompareAttributes
        {
            Id = Id,
            Method = Method,
            Geometry = Geometry!.EqualsExact(other.Geometry) ? null : Geometry,
            StartNodeId = StartNodeId == other.StartNodeId ? null : StartNodeId,
            EndNodeId = EndNodeId == other.EndNodeId ? null : EndNodeId,
            AccessRestriction = AccessRestriction == other.AccessRestriction ? null : AccessRestriction,
            Category = Category == other.Category ? null : Category,
            MaintenanceAuthority = MaintenanceAuthority == other.MaintenanceAuthority ? null : MaintenanceAuthority,
            Morphology = Morphology == other.Morphology ? null : Morphology,
            Status = Status == other.Status ? null : Status,
            LeftSideStreetNameId = LeftSideStreetNameId == other.LeftSideStreetNameId ? null : LeftSideStreetNameId,
            RightSideStreetNameId = RightSideStreetNameId == other.RightSideStreetNameId ? null : RightSideStreetNameId
        };
    }
}
