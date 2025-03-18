namespace RoadRegistry.BackOffice.Abstractions.Exceptions;

using System.Runtime.Serialization;
using BackOffice.Exceptions;

[Serializable]
public class RoadSegmentsNotFoundException : RoadRegistryException
{
    public ICollection<RoadSegmentId> RoadSegmentIds { get; }

    public RoadSegmentsNotFoundException(ICollection<RoadSegmentId> roadSegmentIds) : base($"Road segments could not be found: {string.Join(",", roadSegmentIds)}")
    {
        RoadSegmentIds = roadSegmentIds;
    }

    protected RoadSegmentsNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
