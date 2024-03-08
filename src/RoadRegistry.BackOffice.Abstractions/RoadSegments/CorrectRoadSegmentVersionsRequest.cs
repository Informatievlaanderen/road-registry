namespace RoadRegistry.BackOffice.Abstractions.RoadSegments;

using MediatR;

public sealed record CorrectRoadSegmentVersionsRequest(ICollection<CorrectRoadSegmentVersion>? RoadSegments = null) : IRequest<CorrectRoadSegmentVersionsResponse>;

public sealed record CorrectRoadSegmentVersion(int Id, int? Version, int? GeometryVersion);
