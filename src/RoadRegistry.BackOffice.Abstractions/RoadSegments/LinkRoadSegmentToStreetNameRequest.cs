namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

public sealed record LinkRoadSegmentToStreetNameRequest(int RoadSegmentId, string? LeftStreetNameId, string? RightStreetNameId) : EndpointRequest<LinkRoadSegmentToStreetNameResponse>;
