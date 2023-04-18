namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using MediatR;

public sealed record CorrectRoadNodeVersionsRequest : IRequest<ETagResponse>;
