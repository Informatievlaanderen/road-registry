namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

public sealed record DeleteRoadSegmentsRequest(ICollection<RoadSegmentId> RoadSegmentIds) : EndpointRequest<DeleteRoadSegmentsResponse>;
