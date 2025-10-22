namespace RoadRegistry.BackOffice.Abstractions;

using RoadSegment.ValueObjects;

public interface IHasRoadSegmentId
{
    public RoadSegmentId RoadSegmentId { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
}
