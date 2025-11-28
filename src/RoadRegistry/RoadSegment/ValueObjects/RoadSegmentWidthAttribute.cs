namespace RoadRegistry.RoadSegment.ValueObjects;

using Newtonsoft.Json;
using RoadRegistry.ValueObjects;

public class RoadSegmentWidthAttribute : RoadSegmentDynamicAttribute
{
    public RoadSegmentWidthAttribute(
        AttributeId id,
        RoadSegmentPosition from,
        RoadSegmentPosition to,
        RoadSegmentWidth width
    ) : base(id, from, to)
    {
        Width = width;
    }

    [JsonConstructor]
    private RoadSegmentWidthAttribute(
        int id,
        decimal from,
        decimal to,
        int width
    ) : base(new AttributeId(id), new RoadSegmentPosition(from), new RoadSegmentPosition(to))
    {
        Width = new RoadSegmentWidth(width);
    }

    public RoadSegmentWidth Width { get; }
}
