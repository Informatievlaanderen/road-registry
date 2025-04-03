namespace RoadRegistry.BackOffice.FeatureCompare.V1.Models;

using NetTopologySuite.Geometries;

public record RoadSegmentFeatureCompareAttributes
{
    public RoadSegmentId Id { get; init; }
    public RoadNodeId StartNodeId { get; init; }
    public OrganizationId MaintenanceAuthority { get; init; }
    public RoadNodeId EndNodeId { get; init; }
    public StreetNameLocalId LeftStreetNameId { get; init; }
    public RoadSegmentGeometryDrawMethod Method { get; init; }
    public RoadSegmentMorphology Morphology { get; init; }
    public StreetNameLocalId RightStreetNameId { get; init; }
    public RoadSegmentStatus Status { get; init; }
    public RoadSegmentAccessRestriction AccessRestriction { get; init; }
    public RoadSegmentCategory Category { get; init; }

    public MultiLineString Geometry { get; init; }
}
