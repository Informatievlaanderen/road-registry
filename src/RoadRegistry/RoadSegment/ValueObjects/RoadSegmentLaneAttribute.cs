namespace RoadRegistry.RoadSegment.ValueObjects;

using BackOffice;
using Newtonsoft.Json;

public class RoadSegmentLaneAttribute : RoadSegmentDynamicAttribute
{
    public RoadSegmentLaneAttribute(
        AttributeId id,
        RoadSegmentPosition from,
        RoadSegmentPosition to,
        RoadSegmentLaneCount count,
        RoadSegmentLaneDirection direction
    ) : base(id, from, to)
    {
        Count = count;
        Direction = direction;
    }

    [JsonConstructor]
    private RoadSegmentLaneAttribute(
        int id,
        decimal from,
        decimal to,
        int count,
        string direction
    ) : base(new AttributeId(id), new RoadSegmentPosition(from), new RoadSegmentPosition(to))
    {
        Count = new RoadSegmentLaneCount(count);
        Direction = RoadSegmentLaneDirection.Parse(direction);
    }

    public RoadSegmentLaneCount Count { get; }
    public RoadSegmentLaneDirection Direction { get; }
}
