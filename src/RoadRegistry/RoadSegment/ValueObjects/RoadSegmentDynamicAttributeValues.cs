namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Extensions;
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

    public RoadSegmentDynamicAttributeValues(T value)
    {
        Add(value);
    }

    public RoadSegmentDynamicAttributeValues(IEnumerable<(RoadSegmentPositionCoverage? Coverage, RoadSegmentAttributeSide Side, T Value)> values)
    {
        Values = Values.AddRange(values
            .OrderBy(x => x.Coverage?.From)
            .Select(x => new RoadSegmentDynamicAttributeValue<T>
            {
                Coverage = x.Coverage,
                Side = x.Side,
                Value = x.Value
            }));
    }

    public RoadSegmentDynamicAttributeValues<T> Add(T value)
    {
        Values = Values.Add(new RoadSegmentDynamicAttributeValue<T>
        {
            Coverage = null,
            Side = RoadSegmentAttributeSide.Both,
            Value = value
        });
        return this;
    }

    public RoadSegmentDynamicAttributeValues<T> Add(RoadSegmentPosition from, RoadSegmentPosition to, T value)
    {
        return Add(new RoadSegmentPositionCoverage(from, to), value);
    }
    public RoadSegmentDynamicAttributeValues<T> Add(RoadSegmentPositionCoverage? coverage, T value)
    {
        Values = Values.Add(new RoadSegmentDynamicAttributeValue<T>
        {
            Coverage = coverage,
            Side = RoadSegmentAttributeSide.Both,
            Value = value
        });
        return this;
    }

    public RoadSegmentDynamicAttributeValues<T> Add(RoadSegmentPosition from, RoadSegmentPosition to, RoadSegmentAttributeSide side, T value)
    {
        return Add(new RoadSegmentPositionCoverage(from, to), side, value);
    }
    public RoadSegmentDynamicAttributeValues<T> Add(RoadSegmentPositionCoverage? coverage, RoadSegmentAttributeSide side, T value)
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

    public RoadSegmentDynamicAttributeValues<T> TryCleanEntireLengthCoverages(double geometryLength)
    {
        Values = Values
            .Select(x =>
            {
                if (x.Coverage is not null && x.Coverage.From == RoadSegmentPosition.Zero && x.Coverage.To.IsReasonablyEqualTo(geometryLength))
                {
                    return new RoadSegmentDynamicAttributeValue<T>
                    {
                        Coverage = null,
                        Side = x.Side,
                        Value = x.Value
                    };
                }

                return x;
            })
            .ToImmutableList();
        return this;
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
                    Coverage = x.Coverage ?? new(
                        new RoadSegmentPosition(0),
                        RoadSegmentPosition.FromDouble(thisGeometryLength)
                    ),
                    Side = x.Side,
                    Value = x.Value
                })
                : Values.Reverse().Select(x => new RoadSegmentDynamicAttributeValue<T>
                {
                    Coverage = new(
                        x.Coverage?.To is not null ? RoadSegmentPosition.FromDouble(thisGeometryLength - x.Coverage.To) : new RoadSegmentPosition(0),
                        x.Coverage?.From is not null ? RoadSegmentPosition.FromDouble(thisGeometryLength - x.Coverage.From) : RoadSegmentPosition.FromDouble(thisGeometryLength)
                    ),
                    Side = x.Side,
                    Value = x.Value
                }))
            .Concat(otherSegmentHasIdealDirection
                ? otherAttributes.Values.Select(x => new RoadSegmentDynamicAttributeValue<T>
                {
                    Coverage = new(
                        RoadSegmentPosition.FromDouble(thisGeometryLength + x.Coverage?.From ?? new RoadSegmentPosition(0)),
                        RoadSegmentPosition.FromDouble(thisGeometryLength + x.Coverage?.To ?? RoadSegmentPosition.FromDouble(otherGeometryLength))
                    ),
                    Side = x.Side,
                    Value = x.Value
                })
                : otherAttributes.Values.Reverse().Select(x => new RoadSegmentDynamicAttributeValue<T>
                {
                    Coverage = x.Coverage is not null
                        ? new(
                            RoadSegmentPosition.FromDouble(thisGeometryLength + otherGeometryLength - x.Coverage.To),
                            RoadSegmentPosition.FromDouble(thisGeometryLength + otherGeometryLength - x.Coverage.From)
                        )
                        : new(
                            RoadSegmentPosition.FromDouble(thisGeometryLength + 0),
                            RoadSegmentPosition.FromDouble(thisGeometryLength + otherGeometryLength)
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
                    Coverage = new (previousItem.Coverage!.From, currentItem.Coverage!.To),
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
            var distance = item.Coverage!.To.ToDouble() - item.Coverage!.From.ToDouble();
            if (distance.IsReasonablyEqualTo(segmentLength))
            {
                mergedItems[i] = new RoadSegmentDynamicAttributeValue<T>
                {
                    Coverage = null,
                    Side = item.Side,
                    Value = item.Value
                };
            }
        }

        return new RoadSegmentDynamicAttributeValues<T>(mergedItems);
    }
}
