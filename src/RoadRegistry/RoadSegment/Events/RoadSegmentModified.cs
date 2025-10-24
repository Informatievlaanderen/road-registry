namespace RoadRegistry.RoadSegment.Events;

using System.Collections.Generic;
using BackOffice;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadNetwork.ValueObjects;
using ValueObjects;

public class RoadSegmentModified: IHaveHash
{
    public const string EventName = "RoadSegmentModified";

    public required RoadSegmentId Id { get; init; }
    public required RoadSegmentId? OriginalId { get; init; }
    public required GeometryObject Geometry { get; init; }
    public required RoadNodeId StartNodeId { get; init; }
    public required RoadNodeId EndNodeId { get; init; }
    public required RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; init; }
    public required RoadSegmentDynamicAttributeCollection<RoadSegmentAccessRestriction> AccessRestriction { get; set; }
    public required RoadSegmentDynamicAttributeCollection<RoadSegmentCategory> Category { get; set; }
    public required RoadSegmentDynamicAttributeCollection<RoadSegmentMorphology> Morphology { get; set; }
    public required RoadSegmentDynamicAttributeCollection<RoadSegmentStatus> Status { get; set; }
    public required RoadSegmentDynamicAttributeCollection<StreetNameLocalId> StreetNameId { get; set; }
    public required RoadSegmentDynamicAttributeCollection<OrganizationId> MaintenanceAuthorityId { get; set; }
    public required RoadSegmentDynamicAttributeCollection<RoadSegmentSurfaceType> SurfaceType { get; set; }
    public required IReadOnlyCollection<EuropeanRoadNumber> EuropeanRoadNumbers { get; set; }
    public required IReadOnlyCollection<NationalRoadNumber> NationalRoadNumbers { get; set; }
    //public required bool ConvertedFromOutlined { get; init; }

    public System.Collections.Generic.IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
