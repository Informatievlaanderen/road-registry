namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using BackOffice.Exceptions;

public class RoadSegmentsNotFoundException : RoadRegistryException
{
    public ICollection<RoadSegmentId> RoadSegmentIds { get; }

    public RoadSegmentsNotFoundException(ICollection<RoadSegmentId> roadSegmentIds)
        : base($"Road segments could not be found: {string.Join(",", roadSegmentIds)}")
    {
        RoadSegmentIds = roadSegmentIds;
    }
}
