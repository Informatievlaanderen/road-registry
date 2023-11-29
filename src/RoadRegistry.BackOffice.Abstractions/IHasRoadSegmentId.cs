namespace RoadRegistry.BackOffice.Abstractions;

public interface IHasRoadSegmentId
{
    public RoadSegmentId RoadSegmentId { get; }
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod { get; }
}
