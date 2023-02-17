namespace RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;

using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using MediatR;

public sealed record DeleteRoadSegmentOutlineRequest(int WegsegmentId) : IRequest<ETagResponse>;
