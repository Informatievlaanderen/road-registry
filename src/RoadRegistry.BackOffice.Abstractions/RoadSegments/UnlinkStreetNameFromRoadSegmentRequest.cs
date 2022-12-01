namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

public sealed record UnlinkStreetNameFromRoadSegmentRequest(int WegsegmentId, string? LinkerstraatnaamId, string? RechterstraatnaamId) : EndpointRequest<UnlinkStreetNameFromRoadSegmentResponse>;
