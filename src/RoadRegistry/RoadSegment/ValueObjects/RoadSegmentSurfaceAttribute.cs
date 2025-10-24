namespace RoadRegistry.RoadSegment.ValueObjects;

using BackOffice;
using Newtonsoft.Json;

public class RoadSegmentSurfaceAttribute : RoadSegmentDynamicAttribute
{
    public RoadSegmentSurfaceAttribute(
        AttributeId id,
        RoadSegmentPosition from,
        RoadSegmentPosition to,
        RoadSegmentSurfaceType type
    ) : base(id, from, to)
    {
        Type = type;
    }

    [JsonConstructor]
    private RoadSegmentSurfaceAttribute(
        int id,
        decimal from,
        decimal to,
        string type
    ) : base(new AttributeId(id), new RoadSegmentPosition(from), new RoadSegmentPosition(to))
    {
        Type = RoadSegmentSurfaceType.Parse(type);
    }

    public RoadSegmentSurfaceType Type { get; }
}
