namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using System.ComponentModel.DataAnnotations;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using MediatR;

public sealed record ChangeRoadSegmentAttributesRequest() : IRequest<ETagResponse>
{
    public ChangeRoadSegmentAttributeRequest[] ChangeRequests { get; set; }
}

public abstract record ChangeRoadSegmentAttributeRequest(RoadSegmentId Id);

public record ChangeRoadSegmentStatusAttributeRequest(RoadSegmentId Id, RoadSegmentStatus Status) : ChangeRoadSegmentAttributeRequest(Id);
public record ChangeRoadSegmentMorphologyAttributeRequest(RoadSegmentId Id, RoadSegmentMorphology Morphology) : ChangeRoadSegmentAttributeRequest(Id);
public record ChangeRoadSegmentAccessRestrictionAttributeRequest(RoadSegmentId Id, RoadSegmentAccessRestriction AccessRestriction) : ChangeRoadSegmentAttributeRequest(Id);
public record ChangeRoadSegmentMaintenanceAuthorityAttributeRequest(RoadSegmentId Id, OrganizationId MaintenanceAuthority) : ChangeRoadSegmentAttributeRequest(Id);
public record ChangeRoadSegmentCategoryAttributeRequest(RoadSegmentId Id, RoadSegmentCategory Category) : ChangeRoadSegmentAttributeRequest(Id);
