namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// A road segment's geometry, road nodes and/or dynamically segmented attributes changed. Only what changed is
// carried; a null property means "unchanged". The status and the geometry draw method never travel on this event:
// a change of status is recorded by the status change events (see RoadSegmentStatusChange and RoadSegmentStatusPaths)
// and a change of draw method by RoadSegmentWasMeasured or RoadSegmentWasCorrectedToOutlined.
public record RoadSegmentWasChanged : IMartenEvent
{
    public const string EventName = "RoadSegmentWasChanged"; // BE CAREFUL CHANGING THIS!!

    public required RoadSegmentId RoadSegmentId { get; init; }
    public RoadSegmentIdReference OriginalRoadSegmentIdReference { get; init; }
    public RoadSegmentGeometry? Geometry { get; init; }

    // Null means "the nodes did not change"; a value carries the full new node state, so a
    // RoadSegmentNodeIds with both ids null detaches the segment from its road nodes.
    public RoadSegmentNodeIds? NodeIds { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2>? AccessRestriction { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2>? Category { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>? Morphology { get; init; }
    public RoadSegmentDynamicAttributeValues<StreetNameLocalId>? StreetNameId { get; init; }
    public RoadSegmentDynamicAttributeValues<OrganizationId>? MaintenanceAuthorityId { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2>? SurfaceType { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>? CarTrafficDirection { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>? BikeTrafficDirection { get; init; }
    public RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>? PedestrianTrafficDirection { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}
