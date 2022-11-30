namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

public sealed record LinkRoadSegmentToStreetNameRequest(int WegsegmentId, string? LinkerstraatnaamId, string? RechterstraatnaamId) : EndpointRequest<LinkRoadSegmentToStreetNameResponse>;
