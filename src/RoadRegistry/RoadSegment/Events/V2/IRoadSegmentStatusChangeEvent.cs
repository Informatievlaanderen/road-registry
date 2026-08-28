namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.BackOffice;
using ValueObjects;

// The shape shared by every event a road segment status change raises. Each transition raises an event of its own -
// the status a segment takes on is the event itself, never a field on it - but a reader that only cares about what
// kind of change happened can work from these three interfaces alone, and RoadSegmentStatusChange.ForEvent tells it
// which status the segment ended up in.
public interface IRoadSegmentStatusChangeEvent : IMartenEvent
{
    RoadSegmentId RoadSegmentId { get; }
    ProvenanceData Provenance { get; }
}

// The segment was knotted into the road network and now counts as 'gerealiseerd'.
//
// Everything the action settled is recorded in full rather than as a delta: the geometry as it ended up after being
// snapped onto the road nodes it hooked onto, the two nodes it hangs off, and every dynamically segmented attribute
// remapped onto that geometry. A reader never has to go looking for what the previous value was.
public interface IRoadSegmentWasConnectedEvent : IRoadSegmentStatusChangeEvent
{
    RoadSegmentGeometry Geometry { get; }
    RoadNodeId StartNodeId { get; }
    RoadNodeId EndNodeId { get; }
    RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> AccessRestriction { get; }
    RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> Category { get; }
    RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> Morphology { get; }
    RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; }
    RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; }
    RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> SurfaceType { get; }
    RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> CarTrafficDirection { get; }
    RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> BikeTrafficDirection { get; }
    RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> PedestrianTrafficDirection { get; }
}

// The segment left 'gerealiseerd' and with that came loose from the network.
//
// Only a realized segment is knotted in, so the segment gives up its road nodes here. The nodes it hung off are named
// so a reader can tell what it came loose from; whether those nodes themselves survived is their own business and is
// recorded on them.
//
// Nothing else about the segment changes: the geometry and every attribute stay exactly as they were.
public interface IRoadSegmentWasDisconnectedEvent : IRoadSegmentStatusChangeEvent
{
    RoadNodeId PreviousStartNodeId { get; }
    RoadNodeId PreviousEndNodeId { get; }
}

// The segment moved between two statuses that both leave it outside the network. It carried no road nodes before and
// carries none after, so nothing but the status moves: no geometry, no attributes, no topology.
public interface IRoadSegmentUnconnectedStatusChangeEvent : IRoadSegmentStatusChangeEvent
{
}
