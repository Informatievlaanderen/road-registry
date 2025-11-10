namespace RoadRegistry.RoadSegment.Events;

using System.Collections.Generic;
using BackOffice;
using RoadNetwork.ValueObjects;
using ValueObjects;

public record RoadSegmentModified
{
    public required RoadSegmentId RoadSegmentId { get; init; }
    public RoadSegmentId? OriginalId { get; init; }
    public required GeometryObject Geometry { get; init; }
    public required RoadNodeId StartNodeId { get; init; }
    public required RoadNodeId EndNodeId { get; init; }
    public required RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestriction> AccessRestriction { get; set; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentCategory> Category { get; set; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentMorphology> Morphology { get; set; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentStatus> Status { get; set; }
    public required RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; set; }
    public required RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; set; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceType> SurfaceType { get; set; }
    public required IReadOnlyCollection<EuropeanRoadNumber> EuropeanRoadNumbers { get; set; }
    public required IReadOnlyCollection<NationalRoadNumber> NationalRoadNumbers { get; set; }
    //public required bool ConvertedFromOutlined { get; init; }
}
