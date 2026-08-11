namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// A 'gepland' road segment was knotted into the network and now counts as 'gerealiseerd'. The status the segment
// takes on is the event itself, so it is not carried as a field.
//
// Everything the action settled is recorded in full rather than as a delta: the geometry as it ended up after being
// snapped onto the road nodes it hooked onto, the two nodes it hangs off, and every dynamically segmented attribute
// remapped onto that geometry. A reader never has to go looking for what the previous value was.
public record RoadSegmentWasRealizedFromPlanned : IMartenEvent
{
    public const string EventName = "RoadSegmentWasRealizedFromPlanned"; // BE CAREFUL CHANGING THIS!!

    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }
    public required RoadNodeId StartNodeId { get; init; }
    public required RoadNodeId EndNodeId { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> AccessRestriction { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> Category { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> Morphology { get; init; }
    public required RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; init; }
    public required RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> SurfaceType { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> CarTrafficDirection { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> BikeTrafficDirection { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> PedestrianTrafficDirection { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
