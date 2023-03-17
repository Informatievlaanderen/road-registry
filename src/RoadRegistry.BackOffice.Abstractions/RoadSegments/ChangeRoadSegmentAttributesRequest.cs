namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using System.ComponentModel.DataAnnotations;
using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using MediatR;

public sealed record ChangeRoadSegmentAttributesRequest() : IRequest<ETagResponse>
{
    private readonly List<ChangeRoadSegmentAttributeRequest> _changeRequests = new();

    public IReadOnlyCollection<ChangeRoadSegmentAttributeRequest> ChangeRequests
    {
        get => _changeRequests.ToArray();
        init => _changeRequests = value.ToList();
    }

    public void Add(ChangeRoadSegmentAttributeRequest request) => _changeRequests.Add(request);
}

public abstract record ChangeRoadSegmentAttributeRequest(RoadSegmentId Id);

public record ChangeRoadSegmentMaintenanceAuthorityAttributeRequest(RoadSegmentId Id, OrganizationId MaintenanceAuthority) : ChangeRoadSegmentAttributeRequest(Id);
public record ChangeRoadSegmentStatusAttributeRequest(RoadSegmentId Id, RoadSegmentStatus Status) : ChangeRoadSegmentAttributeRequest(Id);
public record ChangeRoadSegmentMorphologyAttributeRequest(RoadSegmentId Id, RoadSegmentMorphology Morphology) : ChangeRoadSegmentAttributeRequest(Id);
public record ChangeRoadSegmentAccessRestrictionAttributeRequest(RoadSegmentId Id, RoadSegmentAccessRestriction AccessRestriction) : ChangeRoadSegmentAttributeRequest(Id);
public record ChangeRoadSegmentCategoryAttributeRequest(RoadSegmentId Id, RoadSegmentCategory Category) : ChangeRoadSegmentAttributeRequest(Id);
