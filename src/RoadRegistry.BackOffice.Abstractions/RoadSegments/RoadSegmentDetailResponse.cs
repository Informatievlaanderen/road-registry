namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

public sealed record RoadSegmentDetailResponse(
    int RoadSegmentId,
    DateTime BeginTime,
    int? LeftStreetNameId,
    string? LeftStreetName,
    int? RightStreetNameId,
    string? RightStreetName,
    string? LastEventHash
) : EndpointResponse;
