namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using MediatR;

public sealed record MigrateOutlinedRoadSegmentsOutOfRoadNetworkRequest() : IRequest<MigrateOutlinedRoadSegmentsOutOfRoadNetworkResponse>;
