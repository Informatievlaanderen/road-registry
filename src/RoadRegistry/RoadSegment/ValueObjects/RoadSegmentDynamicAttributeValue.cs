namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using Newtonsoft.Json;
using RoadRegistry.ValueObjects;

public sealed class RoadSegmentDynamicAttributeValue<T> : IEquatable<RoadSegmentDynamicAttributeValue<T>>
    where T : notnull
{
    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentAttributeSide Side { get; init; }

    [JsonProperty(DefaultValueHandling = DefaultValueHandling.Ignore)]
    public RoadSegmentPositionCoverage? Coverage { get; init; }

    public required T Value { get; init; }

    public bool Equals(RoadSegmentDynamicAttributeValue<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        return Side.Equals(other.Side)
               && object.Equals(Coverage, other.Coverage)
               && Value.Equals(other.Value)
            ;
    }
}

public sealed record RoadSegmentPositionCoverage(RoadSegmentPosition From, RoadSegmentPosition To);
