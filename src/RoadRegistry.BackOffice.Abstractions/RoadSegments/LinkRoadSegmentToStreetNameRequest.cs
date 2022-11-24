namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

public sealed record LinkRoadSegmentToStreetNameRequest(int RoadSegmentId, int LeftStreetNameId, int RightStreetNameId) : EndpointRequest<LinkRoadSegmentToStreetNameResponse>;
