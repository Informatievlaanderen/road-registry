namespace RoadRegistry.BackOffice.Abstractions.RoadNodes;

using MediatR;

public sealed record CorrectRoadNodeVersionsRequest : IRequest<CorrectRoadNodeVersionsResponse>;
