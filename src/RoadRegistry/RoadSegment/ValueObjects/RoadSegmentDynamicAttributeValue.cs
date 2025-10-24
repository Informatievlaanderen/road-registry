namespace RoadRegistry.RoadSegment.ValueObjects;

using Newtonsoft.Json;
using RoadRegistry.BackOffice;

public class RoadSegmentDynamicAttributeValue<T>
{
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentAttributeSide Side { get; init; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentPosition? From { get; init; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentPosition? To { get; init; }

    public T Value { get; init; }
}
