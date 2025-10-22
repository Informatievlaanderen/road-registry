namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using RoadSegment.ValueObjects;

public sealed record DeleteRoadSegmentsRequest(ICollection<RoadSegmentId> RoadSegmentIds) : EndpointRequest<DeleteRoadSegmentsResponse>;
