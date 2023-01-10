namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

public sealed record RoadSegmentDetailRequest(int WegsegmentId) : EndpointRequest<RoadSegmentDetailResponse>
{
}
