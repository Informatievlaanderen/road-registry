namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

public sealed record LinkStreetNameRequest(int WegsegmentId, string? LinkerstraatnaamId, string? RechterstraatnaamId) : EndpointRequest<LinkStreetNameResponse>;
