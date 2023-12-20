namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using MediatR;

public sealed record CorrectRoadSegmentVersionsRequest(ICollection<int>? RoadSegmentIds = null) : IRequest<CorrectRoadSegmentVersionsResponse>;
