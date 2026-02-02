namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using RoadRegistry.ValueObjects;

public sealed class RoadSegmentDynamicAttributeValues<T> : IEquatable<RoadSegmentDynamicAttributeValues<T>>
    where T : notnull
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

    public RoadSegmentDynamicAttributeValues(T value, RoadSegmentGeometry geometry)
    {
        Add(value, geometry);
    }

    public RoadSegmentDynamicAttributeValues(IEnumerable<(RoadSegmentPositionCoverage Coverage, RoadSegmentAttributeSide Side, T Value)> values)
    {
        Values = Values.AddRange(values
            .OrderBy(x => x.Coverage.From)
            .Select(x => new RoadSegmentDynamicAttributeValue<T>
            {
                Coverage = x.Coverage,
                Side = x.Side,
                Value = x.Value
            }));
    }

    public RoadSegmentDynamicAttributeValues<T> Add(T value, RoadSegmentGeometry geometry)
    {
        return Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(geometry.Value.Length), value);
    }

    public RoadSegmentDynamicAttributeValues<T> Add(RoadSegmentPositionV2 from, RoadSegmentPositionV2 to, T value)
    {
        return Add(new RoadSegmentPositionCoverage(from, to), value);
    }
    public RoadSegmentDynamicAttributeValues<T> Add(RoadSegmentPositionCoverage coverage, T value)
    {
        return Add(coverage, RoadSegmentAttributeSide.Both, value);
    }

    public RoadSegmentDynamicAttributeValues<T> Add(RoadSegmentPositionV2 from, RoadSegmentPositionV2 to, RoadSegmentAttributeSide side, T value)
    {
        return Add(new RoadSegmentPositionCoverage(from, to), side, value);
    }
    public RoadSegmentDynamicAttributeValues<T> Add(RoadSegmentPositionCoverage coverage, RoadSegmentAttributeSide side, T value)
    {
        Values = Values.Add(new RoadSegmentDynamicAttributeValue<T>
        {
            Coverage = coverage,
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
                    Coverage = x.Coverage,
                    Side = x.Side,
                    Value = x.Value
                })
                : Values.Reverse().Select(x => new RoadSegmentDynamicAttributeValue<T>
                {
                    Coverage = new(
                        new RoadSegmentPositionV2(thisGeometryLength - x.Coverage.To),
                        new RoadSegmentPositionV2(thisGeometryLength - x.Coverage.From)
                    ),
                    Side = x.Side,
                    Value = x.Value
                }))
            .Concat(otherSegmentHasIdealDirection
                ? otherAttributes.Values.Select(x => new RoadSegmentDynamicAttributeValue<T>
                {
                    Coverage = new(
                        new RoadSegmentPositionV2(thisGeometryLength + x.Coverage.From),
                        new RoadSegmentPositionV2(thisGeometryLength + x.Coverage.To)
                    ),
                    Side = x.Side,
                    Value = x.Value
                })
                : otherAttributes.Values.Reverse().Select(x => new RoadSegmentDynamicAttributeValue<T>
                {
                    Coverage = new(
                        new RoadSegmentPositionV2(thisGeometryLength + otherGeometryLength - x.Coverage.To),
                        new RoadSegmentPositionV2(thisGeometryLength + otherGeometryLength - x.Coverage.From)
                    ),
                    Side = x.Side,
                    Value = x.Value
                }))
            .ToList();

        var previousItem = mergedItems[0];
        for (var i = 1; i < mergedItems.Count; i++)
        {
            var currentItem = mergedItems[i];
            if (previousItem.Value.Equals(currentItem.Value) && previousItem.Side.Equals(currentItem.Side))
            {
                previousItem = new RoadSegmentDynamicAttributeValue<T>
                {
                    Coverage = new (previousItem.Coverage.From, currentItem.Coverage.To),
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

        return new RoadSegmentDynamicAttributeValues<T>(mergedItems);
    }
}
