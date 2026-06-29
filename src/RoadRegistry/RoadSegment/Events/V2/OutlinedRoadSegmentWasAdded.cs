namespace RoadRegistry.RoadSegment.Events.V2;

using System.Collections.Generic;
using Be.Vlaanderen.Basisregisters.GrAr.Common;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.BackOffice;
using RoadRegistry.RoadSegment.ValueObjects;

public record OutlinedRoadSegmentWasAdded: IMartenEvent, ICreatedEvent
{
    public const string EventName = "OutlinedRoadSegmentWasAdded"; // BE CAREFUL CHANGING THIS!!

    public required RoadSegmentId RoadSegmentId { get; init; }
    public required RoadSegmentGeometry Geometry { get; init; }
    public required RoadSegmentStatusV2 Status { get; init; }
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
