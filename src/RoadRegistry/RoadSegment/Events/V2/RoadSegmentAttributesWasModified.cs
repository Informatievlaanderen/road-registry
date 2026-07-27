namespace RoadRegistry.RoadSegment.Events.V2;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using ValueObjects;
using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using RoadRegistry.BackOffice;

// Emitted by the dedicated 'wijzig attribuutwaarden' action. Unlike RoadSegmentWasModified (which can also change
// geometry/status/nodes), this event only carries the dynamically-segmented attributes; every field is nullable so a
// null means "leave this attribute unchanged".
public record RoadSegmentAttributesWasModified : IMartenEvent
{
    public const string EventName = "RoadSegmentAttributesWasModified"; // BE CAREFUL CHANGING THIS!!

    public required RoadSegmentId RoadSegmentId { get; init; }
    public RoadSegmentIdReference OriginalRoadSegmentIdReference { get; init; }
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
