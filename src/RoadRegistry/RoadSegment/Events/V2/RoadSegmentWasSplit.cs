namespace RoadRegistry.RoadSegment.Events.V2;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.BackOffice;
using ValueObjects;

public record RoadSegmentWasSplit : IMartenEvent
{
    public const string EventName = "RoadSegmentWasSplit"; // BE CAREFUL CHANGING THIS!!

    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadSegmentId[] NewRoadSegmentIds { get; init; }

    // Only present when the split segment keeps its identifier for the largest resulting part
    // (situation where one part covers >= 70% of the original geometry). In that case this
    // records the changes the kept segment underwent because of the split.
    public RoadSegmentSplitModifications? Modifications { get; init; }

    public required ProvenanceData Provenance { get; init; }

    public IEnumerable<string> GetHashFields() => ObjectHasher.GetHashFields(this);
    public string GetHash() => this.ToEventHash(EventName);
}

public record RoadSegmentSplitModifications
{
    public required RoadSegmentGeometry Geometry { get; init; }
    public required RoadNodeId? StartNodeId { get; init; }
    public required RoadNodeId? EndNodeId { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentAccessRestrictionV2> AccessRestriction { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentCategoryV2> Category { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2> Morphology { get; init; }
    public required RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNameId { get; init; }
    public required RoadSegmentDynamicAttributeValues<OrganizationId> MaintenanceAuthorityId { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentSurfaceTypeV2> SurfaceType { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> CarTrafficDirection { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> BikeTrafficDirection { get; init; }
    public required RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection> PedestrianTrafficDirection { get; init; }
}
