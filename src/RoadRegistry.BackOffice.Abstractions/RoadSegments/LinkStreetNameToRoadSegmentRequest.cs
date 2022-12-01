namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

public sealed record LinkStreetNameToRoadSegmentRequest(int WegsegmentId, string? LinkerstraatnaamId, string? RechterstraatnaamId) : EndpointRequest<LinkStreetNameToRoadSegmentResponse>;
