namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

public sealed record DeleteRoadSegmentsRequest(ICollection<int> RoadSegmentIds) : EndpointRequest<DeleteRoadSegmentsResponse>;
