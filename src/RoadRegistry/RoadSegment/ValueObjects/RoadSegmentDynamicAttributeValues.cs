namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using RoadRegistry.ValueObjects;

public class RoadSegmentDynamicAttributeValues<T> : IEquatable<RoadSegmentDynamicAttributeValues<T>>
{
    public ImmutableList<RoadSegmentDynamicAttributeValue<T>> Values { get; private set; } = [];

    public RoadSegmentDynamicAttributeValues()
    {
    }

    [JsonConstructor]
    public RoadSegmentDynamicAttributeValues(ICollection<RoadSegmentDynamicAttributeValue<T>> values)
    {
        Values = ImmutableList.CreateRange(values);
    }

    public RoadSegmentDynamicAttributeValues(T value)
    {
        Add(value);
    }

    public RoadSegmentDynamicAttributeValues(IEnumerable<(RoadSegmentPosition From, RoadSegmentPosition To, RoadSegmentAttributeSide Side, T Value)> values)
    {
        Values = Values.AddRange(values
            .OrderBy(x => x.From)
            .Select(x => new RoadSegmentDynamicAttributeValue<T>
            {
                From = x.From,
                To = x.To,
                Side = x.Side,
                Value = x.Value
            }));
    }

    public RoadSegmentDynamicAttributeValues(IEnumerable<(RoadSegmentPosition? From, RoadSegmentPosition? To, RoadSegmentAttributeSide Side, T Value)> values)
    {
        Values = Values.AddRange(values
            .OrderBy(x => x.From)
            .Select(x => new RoadSegmentDynamicAttributeValue<T>
            {
                From = x.From,
                To = x.To,
                Side = x.Side,
                Value = x.Value
            }));
    }

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

    public RoadSegmentDynamicAttributeValues<T> Add(RoadSegmentPosition? from, RoadSegmentPosition? to, T value)
    {
        Values = Values.Add(new RoadSegmentDynamicAttributeValue<T>
        {
            From = from,
            To = to,
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
