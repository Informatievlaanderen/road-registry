namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using MediatR;

public sealed record ChangeRoadSegmentAttributesRequest : IRequest<ETagResponse>
{
    private readonly List<ChangeRoadSegmentAttributeRequest> _changeRequests = new();

    public IReadOnlyCollection<ChangeRoadSegmentAttributeRequest> ChangeRequests
    {
        get => _changeRequests.ToArray();
        init => _changeRequests = value.ToList();
    }

    public ChangeRoadSegmentAttributesRequest Add(RoadSegmentId roadSegmentId, Action<ChangeRoadSegmentAttributeRequest> updateRoadSegment)
    {
        var request = _changeRequests.SingleOrDefault(x => x.Id == roadSegmentId);
        if (request is null)
        {
            request = new ChangeRoadSegmentAttributeRequest { Id = roadSegmentId };
            _changeRequests.Add(request);
        }

        updateRoadSegment(request);
        return this;
    }
}

public class ChangeRoadSegmentAttributeRequest
{
    public RoadSegmentId Id { get; set; }
    public OrganizationId? MaintenanceAuthority { get; set; }
    public RoadSegmentStatus? Status { get; set; }
    public RoadSegmentMorphology? Morphology { get; set; }
    public RoadSegmentAccessRestriction? AccessRestriction { get; set; }
    public RoadSegmentCategory? Category { get; set; }
    public ICollection<EuropeanRoadNumber>? EuropeanRoads { get; set; }
    public ICollection<NationalRoadNumber>? NationalRoads { get; set; }
    public ICollection<ChangeRoadSegmentNumberedRoadAttribute>? NumberedRoads { get; set; }
}

public class ChangeRoadSegmentNumberedRoadAttribute
{
    public NumberedRoadNumber Number { get; set; }
    public RoadSegmentNumberedRoadDirection Direction { get; set; }
    public RoadSegmentNumberedRoadOrdinal Ordinal { get; set; }
}
