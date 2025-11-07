namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using System.Collections.Immutable;
using System.Linq;
using RoadRegistry.BackOffice;

public class RoadSegmentDynamicAttributeValues<T> : IEquatable<RoadSegmentDynamicAttributeValues<T>>
{
    public ImmutableList<RoadSegmentDynamicAttributeValue<T>> Values { get; private set; } = [];

    public RoadSegmentDynamicAttributeValues<T> Add(T value)
    {
        Values = Values.Add(new RoadSegmentDynamicAttributeValue<T>
        {
            From = null,
            To = null,
            Side = RoadSegmentAttributeSide.Both,
            Value = value
        });
        return this;
    }

    public RoadSegmentDynamicAttributeValues<T> Add(RoadSegmentPosition? from, RoadSegmentPosition? to, RoadSegmentAttributeSide side, T value)
    {
        Values = Values.Add(new RoadSegmentDynamicAttributeValue<T>
        {
            From = from,
            To = to,
            Side = side,
            Value = value
        });
        return this;
    }

    public bool Equals(RoadSegmentDynamicAttributeValues<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        return Values.SequenceEqual(other.Values);
    }
}
