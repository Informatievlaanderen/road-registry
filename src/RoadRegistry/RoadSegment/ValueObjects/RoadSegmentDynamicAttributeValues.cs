namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Extensions;
using Newtonsoft.Json;
using RoadRegistry.ValueObjects;

public sealed class RoadSegmentDynamicAttributeValues<T> : IEquatable<RoadSegmentDynamicAttributeValues<T>>
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

    public RoadSegmentDynamicAttributeValues<T> MergeWith(RoadSegmentDynamicAttributeValues<T> otherAttributes,
        double thisGeometryLength, double otherGeometryLength,
        bool thisSegmentHasIdealDirection, bool otherSegmentHasIdealDirection)
    {
        // ensure all from/to are not nullable for easier checking + in the correct order with correct from/to values related to their direction and if segment2 or not
        var mergedItems = Enumerable.Empty<RoadSegmentDynamicAttributeValue<T>>()
            .Concat(thisSegmentHasIdealDirection
                    ? Values.Select(x => new RoadSegmentDynamicAttributeValue<T>
                        {
                            From = x.From ?? new RoadSegmentPosition(0),
                            To = x.To ?? RoadSegmentPosition.FromDouble(thisGeometryLength),
                            Side = x.Side,
                            Value = x.Value
                        })
                    : Values.Reverse().Select(x => new RoadSegmentDynamicAttributeValue<T>
                        {
                            From = x.To is not null ? RoadSegmentPosition.FromDouble(thisGeometryLength - x.To.Value) : new RoadSegmentPosition(0),
                            To = x.From is not null ? RoadSegmentPosition.FromDouble(thisGeometryLength - x.From.Value) : RoadSegmentPosition.FromDouble(thisGeometryLength),
                            Side = x.Side,
                            Value = x.Value
                        }))
            .Concat(otherSegmentHasIdealDirection
                    ? otherAttributes.Values.Select(x => new RoadSegmentDynamicAttributeValue<T>
                        {
                            From = RoadSegmentPosition.FromDouble(thisGeometryLength + x.From ?? new RoadSegmentPosition(0)),
                            To = RoadSegmentPosition.FromDouble(thisGeometryLength + x.To ?? RoadSegmentPosition.FromDouble(otherGeometryLength)),
                            Side = x.Side,
                            Value = x.Value
                        })
                    : otherAttributes.Values.Reverse().Select(x => new RoadSegmentDynamicAttributeValue<T>
                    {
                        From = x.To is not null ? RoadSegmentPosition.FromDouble(thisGeometryLength + otherGeometryLength - x.To.Value) : RoadSegmentPosition.FromDouble(thisGeometryLength + 0),
                        To = x.From is not null ? RoadSegmentPosition.FromDouble(thisGeometryLength + otherGeometryLength - x.From.Value) : RoadSegmentPosition.FromDouble(thisGeometryLength + otherGeometryLength),
                        Side = x.Side,
                        Value = x.Value
                    }))
            .ToList();

        var previousItem = mergedItems[0];
        for (var i = 1; i < mergedItems.Count; i++)
        {
            var currentItem = mergedItems[i];
            if (previousItem.Value!.Equals(currentItem.Value) && previousItem.Side.Equals(currentItem.Side))
            {
                previousItem = new RoadSegmentDynamicAttributeValue<T>
                {
                    From = previousItem.From,
                    To = currentItem.To,
                    Side = previousItem.Side,
                    Value = previousItem.Value
                };
                mergedItems[i - 1] = previousItem;
                mergedItems.RemoveAt(i);
            }
            else
            {
                previousItem = currentItem;
            }
        }

        // make coverage null when possible
        var segmentLength = thisGeometryLength + otherGeometryLength;
        for (var i = 0; i < mergedItems.Count; i++)
        {
            var item = mergedItems[i];
            var distance = item.To!.Value.ToDouble() - item.From!.Value.ToDouble();
            if (distance.IsReasonablyEqualTo(segmentLength))
            {
                mergedItems[i] = new RoadSegmentDynamicAttributeValue<T>
                {
                    From = null,
                    To = null,
                    Side = item.Side,
                    Value = item.Value
                };
            }
        }

        return new RoadSegmentDynamicAttributeValues<T>(mergedItems);
    }
}
