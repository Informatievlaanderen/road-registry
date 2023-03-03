namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using System.ComponentModel.DataAnnotations;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using MediatR;

public sealed record ChangeRoadSegmentAttributesRequest(
        [Required] int WegsegmentId,
        RoadSegmentStatus? Status,
        RoadSegmentMorphology? Morphology,
        RoadSegmentAccessRestriction? AccessRestriction,
        OrganizationId? MaintenanceAuthority,
        RoadSegmentCategory? Category)
    : IRequest<ETagResponse>;
