namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using MediatR;

public sealed record CorrectRoadSegmentVersionsRequest : IRequest<CorrectRoadSegmentVersionsResponse>;
