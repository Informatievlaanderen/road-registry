namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using MediatR;

public sealed record ChangeRoadSegmentsRequest : IRequest<ETagResponse>
{
    private readonly List<ChangeRoadSegmentRequest> _changeRequests = new();

    public IReadOnlyCollection<ChangeRoadSegmentRequest> ChangeRequests
    {
        get => _changeRequests.ToArray();
        init => _changeRequests = value.ToList();
    }

    public ChangeRoadSegmentsRequest Add(RoadSegmentId roadSegmentId, Action<ChangeRoadSegmentRequest> updateRoadSegment)
    {
        var request = _changeRequests.SingleOrDefault(x => x.Id == roadSegmentId);
        if (request is null)
        {
            request = new ChangeRoadSegmentRequest { Id = roadSegmentId };
            _changeRequests.Add(request);
        }

        updateRoadSegment(request);
        return this;
    }
}

public class ChangeRoadSegmentRequest
{
    public RoadSegmentId Id { get; set; }
    public ChangeRoadSegmentSurfaceAttributeRequest[]? Surfaces { get; set; }
    public ChangeRoadSegmentWidthAttributeRequest[]? Widths { get; set; }
    public ChangeRoadSegmentLaneAttributeRequest[]? Lanes { get; set; }
}

public sealed record ChangeRoadSegmentSurfaceAttributeRequest
{
    public RoadSegmentPosition FromPosition { get; init; }
    public RoadSegmentPosition ToPosition { get; init; }
    public RoadSegmentSurfaceType Type { get; init; }
}

public sealed record ChangeRoadSegmentWidthAttributeRequest
{
    public RoadSegmentPosition FromPosition { get; init; }
    public RoadSegmentPosition ToPosition { get; init; }
    public RoadSegmentWidth Width { get; init; }
}

public sealed record ChangeRoadSegmentLaneAttributeRequest
{
    public RoadSegmentPosition FromPosition { get; init; }
    public RoadSegmentPosition ToPosition { get; init; }
    public RoadSegmentLaneCount Count { get; init; }
    public RoadSegmentLaneDirection Direction { get; init; }
}
