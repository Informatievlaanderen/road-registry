namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using Newtonsoft.Json;
using RoadRegistry.ValueObjects;

public class RoadSegmentDynamicAttributeValue<T> : IEquatable<RoadSegmentDynamicAttributeValue<T>>
{
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentAttributeSide Side { get; init; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentPosition? From { get; init; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentPosition? To { get; init; }

    public required T Value { get; init; }

    public bool Equals(RoadSegmentDynamicAttributeValue<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        return Side.Equals(other.Side)
               && From.Equals(other.From)
               && To.Equals(other.To)
               && Value.Equals(other.Value)
               ;
    }
}
