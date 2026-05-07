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
    public RoadSegmentDynamicAttributeValues(IReadOnlyList<RoadSegmentDynamicAttributeValue<T>> values)
    {
        Values = Shrink(values);
    }

    public RoadSegmentDynamicAttributeValues(IEnumerable<(RoadSegmentPositionCoverage Coverage, RoadSegmentAttributeSide Side, T Value)> values)
    {
        Values = Shrink(values
            .OrderBy(x => x.Coverage.From)
            .Select(x => new RoadSegmentDynamicAttributeValue<T>
            {
                Coverage = x.Coverage,
                Side = x.Side,
                Value = x.Value
            })
            .ToArray());
    }

    public RoadSegmentDynamicAttributeValues(T value, RoadSegmentGeometry geometry)
    {
        Add(value, geometry);
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
        Values = Shrink(Values.Add(new RoadSegmentDynamicAttributeValue<T>
        {
            Coverage = coverage,
            Side = side,
            Value = value
        }));
        return this;
    }

    public bool Equals(RoadSegmentDynamicAttributeValues<T>? other)
    {
        return Equals(this, other);
    }
    private static bool Equals(RoadSegmentDynamicAttributeValues<T>? left, RoadSegmentDynamicAttributeValues<T>? right)
    {
        if (left is null && right is null)
        {
            return true;
        }

        if (left is null || right is null)
        {
            return false;
        }

        return left.Values.SequenceEqual(right.Values);
    }
    public static bool operator ==(RoadSegmentDynamicAttributeValues<T>? left, RoadSegmentDynamicAttributeValues<T>? right) =>
        Equals(left, right);

    public static bool operator !=(RoadSegmentDynamicAttributeValues<T>? left, RoadSegmentDynamicAttributeValues<T>? right) =>
        !Equals(left, right);

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

        return new RoadSegmentDynamicAttributeValues<T>(mergedItems);
    }

    private static ImmutableList<RoadSegmentDynamicAttributeValue<T>> Shrink(IReadOnlyList<RoadSegmentDynamicAttributeValue<T>> values)
    {
        if (values.Count <= 1)
        {
            return ImmutableList.CreateRange(values);
        }

        var working = values.ToList();

        // Keep applying reductions until the list is stable.
        // Each pass returns true as soon as it makes a single change; we then
        // restart from the top so that earlier passes get another shot at
        // anything that became reducible because of a later pass.
        bool changed;
        do
        {
            changed = TryMergeSameSideSameValue(working)
                      || TryPromoteLeftRightToBoth(working)
                      || TryRemoveSidedEntryContainedInBoth(working);
        } while (changed);

        return working
            .OrderBy(x => x.Coverage.From)
            .ToImmutableList();
    }

    // Merges two entries that have the same Side and Value when their coverages
    // touch or overlap. e.g. Left,V,[0,5] + Left,V,[5,10] -> Left,V,[0,10].
    private static bool TryMergeSameSideSameValue(List<RoadSegmentDynamicAttributeValue<T>> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            for (var j = i + 1; j < items.Count; j++)
            {
                var a = items[i];
                var b = items[j];

                if (a.Side != b.Side) continue;
                if (!ValuesAreEqual(a, b)) continue;
                if (!CoveragesTouchOrOverlap(a.Coverage, b.Coverage)) continue;

                items[i] = new RoadSegmentDynamicAttributeValue<T>
                {
                    Coverage = UnionCoverages(a.Coverage, b.Coverage),
                    Side = a.Side,
                    Value = a.Value
                };
                items.RemoveAt(j);
                return true;
            }
        }

        return false;
    }

    // Combines a Left and Right entry that share the exact same coverage and value
    // into a single Both entry. e.g. Left,V,[0,10] + Right,V,[0,10] -> Both,V,[0,10].
    // Only triggered on an identical coverage to avoid creating *more* entries
    // (which would happen if we tried to promote partial overlaps).
    private static bool TryPromoteLeftRightToBoth(List<RoadSegmentDynamicAttributeValue<T>> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            for (var j = i + 1; j < items.Count; j++)
            {
                var a = items[i];
                var b = items[j];

                if (!ValuesAreEqual(a, b)) continue;
                if (!Equals(a.Coverage, b.Coverage)) continue;
                if (!IsLeftRightPair(a.Side, b.Side)) continue;

                items[i] = new RoadSegmentDynamicAttributeValue<T>
                {
                    Coverage = a.Coverage,
                    Side = RoadSegmentAttributeSide.Both,
                    Value = a.Value
                };
                items.RemoveAt(j);
                return true;
            }
        }

        return false;
    }

    // Drops any Left or Right entry whose coverage is fully contained within
    // a Both entry that has the same value, since the sided entry is redundant.
    private static bool TryRemoveSidedEntryContainedInBoth(List<RoadSegmentDynamicAttributeValue<T>> items)
    {
        for (var i = 0; i < items.Count; i++)
        {
            var a = items[i];
            if (a.Side != RoadSegmentAttributeSide.Both) continue;

            for (var j = 0; j < items.Count; j++)
            {
                if (i == j) continue;

                var b = items[j];
                if (b.Side == RoadSegmentAttributeSide.Both) continue;
                if (!ValuesAreEqual(a, b)) continue;
                if (!CoverageContains(a.Coverage, b.Coverage)) continue;

                items.RemoveAt(j);
                return true;
            }
        }

        return false;
    }

    private static bool ValuesAreEqual(RoadSegmentDynamicAttributeValue<T> a, RoadSegmentDynamicAttributeValue<T> b)
    {
        return a.Value is not null && b.Value is not null && a.Value.Equals(b.Value);
    }

    private static bool CoveragesTouchOrOverlap(RoadSegmentPositionCoverage a, RoadSegmentPositionCoverage b)
    {
        return a.To >= b.From && b.To >= a.From;
    }

    private static bool CoverageContains(RoadSegmentPositionCoverage outer, RoadSegmentPositionCoverage inner)
    {
        return outer.From <= inner.From && outer.To >= inner.To;
    }

    private static RoadSegmentPositionCoverage UnionCoverages(RoadSegmentPositionCoverage a, RoadSegmentPositionCoverage b)
    {
        var from = a.From <= b.From ? a.From : b.From;
        var to = a.To >= b.To ? a.To : b.To;
        return new RoadSegmentPositionCoverage(from, to);
    }

    private static bool IsLeftRightPair(RoadSegmentAttributeSide a, RoadSegmentAttributeSide b)
    {
        return (a == RoadSegmentAttributeSide.Left && b == RoadSegmentAttributeSide.Right)
               || (a == RoadSegmentAttributeSide.Right && b == RoadSegmentAttributeSide.Left);
    }
}
