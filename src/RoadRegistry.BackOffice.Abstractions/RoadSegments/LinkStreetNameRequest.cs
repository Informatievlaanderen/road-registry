namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Responses;
using MediatR;

public sealed record LinkStreetNameRequest(int WegsegmentId, string Methode, string? LinkerstraatnaamId, string? RechterstraatnaamId) : IRequest<ETagResponse>;
