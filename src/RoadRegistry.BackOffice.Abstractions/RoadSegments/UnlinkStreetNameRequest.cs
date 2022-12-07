namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

public sealed record UnlinkStreetNameRequest(int WegsegmentId, string? LinkerstraatnaamId, string? RechterstraatnaamId) : EndpointRequest<UnlinkStreetNameResponse>;
