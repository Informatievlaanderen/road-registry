namespace RoadRegistry.RoadSegment.ValueObjects;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Newtonsoft.Json;
using RoadRegistry.Extensions;
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
        return Add(coverage, RoadSegmentAttributeSide.Beide, value);
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

    // Moves the trailing coverage(s) - the ones ending at the highest ToPosition - onto the given segment length.
    //
    // A trailing coverage that would collapse to zero (or invert) is left alone - snapping must not manufacture an
    // invalid range, and validation should report what is really wrong.
    private RoadSegmentDynamicAttributeValues<T> WithTrailingCoverageSnappedTo(double segmentLength)
    {
        if (Values.Count == 0)
        {
            return this;
        }

        var snappedTo = new RoadSegmentPositionV2(segmentLength);
        var trailingTo = Values.Max(x => x.Coverage.To);
        if (trailingTo == snappedTo)
        {
            return this;
        }

        if (Values.Any(x => x.Coverage.To == trailingTo && x.Coverage.From >= snappedTo))
        {
            return this;
        }

        return new RoadSegmentDynamicAttributeValues<T>(Values.Select(x => (
            Coverage: x.Coverage.To == trailingTo ? x.Coverage with { To = snappedTo } : x.Coverage,
            x.Side,
            Value: x.Value!)));
    }

    // Moves every position onto the geometry a road segment has after one or both of its end vertices were dragged
    // along with a road node.
    //
    // Only the outermost stretches change length - from an end vertex to the vertex next to it - so this is not a
    // scale of the whole segment. A position keeps its relative place between the same two vertices it already sat
    // between, which leaves the untouched stretches covering exactly the length they covered before; they merely shift
    // when a stretch ahead of them grew or shrank. Stretching the segment as a whole would drag attribute boundaries
    // away from the vertices they were placed against, somewhere the geometry did not move at all.
    //
    // The vertex positions are the distance of each vertex from the start of the line, before and after the move.
    //
    // The trailing coverage is landed exactly on the new length rather than on its own remapped value: positions are
    // rounded to the centimetre one by one, and without this the rounding leaves the last position a centimetre off
    // the geometry - which every later change to that segment then rejects with a ToPositionNotEqualToLength error.
    public RoadSegmentDynamicAttributeValues<T> RemapTo(
        IReadOnlyList<double> currentVertexPositions,
        IReadOnlyList<double> newVertexPositions)
    {
        if (Values.Count == 0)
        {
            return this;
        }

        // Only the end vertices move, so the vertex count is the same before and after. Anything else is not a drag
        // and is left alone rather than remapped against a line it does not describe.
        if (currentVertexPositions.Count < 2 || currentVertexPositions.Count != newVertexPositions.Count)
        {
            return this;
        }

        if (currentVertexPositions.SequenceEqual(newVertexPositions))
        {
            return this;
        }

        var remapped = new RoadSegmentDynamicAttributeValues<T>(Values.Select(x => (
            Coverage: new RoadSegmentPositionCoverage(
                new RoadSegmentPositionV2(RemapPosition(x.Coverage.From.ToDouble(), currentVertexPositions, newVertexPositions)),
                new RoadSegmentPositionV2(RemapPosition(x.Coverage.To.ToDouble(), currentVertexPositions, newVertexPositions))),
            x.Side,
            Value: x.Value!)));

        return remapped
            .WithTrailingCoverageSnappedTo(newVertexPositions[^1].RoundToCm())
            .WithoutCoveragesShorterThan(Distances.RoadSegmentDynamicAttributeMinimumLength);
    }

    // A stretch squeezed below the minimum by a geometry change lapses, and the stretch next to it takes over what it
    // covered. Which neighbour absorbs it follows the direction it can grow in: the first stretch is absorbed by the
    // one after it - so that value now runs from the start - and any other by the one before it, which runs on to
    // where the lapsed stretch ended.
    //
    // Sides are handled apart from each other: 'links' and 'rechts' are two independent runs of coverages, and a value
    // that applies to 'beide' is a run of its own.
    private RoadSegmentDynamicAttributeValues<T> WithoutCoveragesShorterThan(double minimumLength)
    {
        if (Values.Count == 0)
        {
            return this;
        }

        var kept = Values
            .GroupBy(x => x.Side)
            .SelectMany(side => WithoutCoveragesShorterThan(
                side.OrderBy(x => x.Coverage.From).Select(x => (x.Coverage, x.Side, Value: x.Value!)).ToList(),
                minimumLength));

        return new RoadSegmentDynamicAttributeValues<T>(kept);
    }

    private static List<(RoadSegmentPositionCoverage Coverage, RoadSegmentAttributeSide Side, T Value)> WithoutCoveragesShorterThan(
        List<(RoadSegmentPositionCoverage Coverage, RoadSegmentAttributeSide Side, T Value)> ordered,
        double minimumLength)
    {
        // A single stretch has no neighbour to lapse into. It covers the whole segment, so its length is the segment's
        // own, which is refused elsewhere if it is too short.
        while (ordered.Count > 1)
        {
            var index = ordered.FindIndex(x => x.Coverage.To.ToDouble() - x.Coverage.From.ToDouble() < minimumLength);
            if (index < 0)
            {
                break;
            }

            var lapsed = ordered[index];
            if (index == 0)
            {
                var next = ordered[1];
                ordered[1] = (next.Coverage with { From = lapsed.Coverage.From }, next.Side, next.Value);
            }
            else
            {
                var previous = ordered[index - 1];
                ordered[index - 1] = (previous.Coverage with { To = lapsed.Coverage.To }, previous.Side, previous.Value);
            }

            ordered.RemoveAt(index);
        }

        return ordered;
    }

    private static double RemapPosition(double position, IReadOnlyList<double> current, IReadOnlyList<double> updated)
    {
        for (var i = 1; i < current.Count; i++)
        {
            if (position > current[i])
            {
                continue;
            }

            var span = current[i] - current[i - 1];
            var ratio = span > 0 ? (position - current[i - 1]) / span : 0;
            return updated[i - 1] + ratio * (updated[i] - updated[i - 1]);
        }

        return updated[^1];
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
        thisGeometryLength = thisGeometryLength.RoundToCm();
        otherGeometryLength = otherGeometryLength.RoundToCm();

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

    // Splits the attribute values at the given position (a measure along the segment, in meters) and lands each part
    // on the length its own geometry actually has.
    //
    // The two are not the same number. The split works in measures along the original line, but the part geometries
    // are rounded to the centimetre, and rounding moves the interpolated cut vertex - so a part can measure a
    // different centimetre than the measure its positions were derived from. Leaving the measure-derived value in
    // place puts the trailing position a centimetre off the part's own geometry, and every later change to that
    // segment then fails with a ToPositionNotEqualToLength error. Hence the two lengths: whoever cuts the geometry
    // has to say what the cut parts came out at.
    public (RoadSegmentDynamicAttributeValues<T> First, RoadSegmentDynamicAttributeValues<T> Second) SplitAt(
        RoadSegmentPositionV2 cutPosition, double totalLength, double firstGeometryLength, double secondGeometryLength)
    {
        var (first, second) = SplitAtMeasure(cutPosition, totalLength);

        return (first.WithTrailingCoverageSnappedTo(firstGeometryLength),
            second.WithTrailingCoverageSnappedTo(secondGeometryLength));
    }

    // Splits the attribute values at the given position (a measure along the segment, in meters).
    // Returns the values for the part before the cut ([0, cut]) and the part after the cut,
    // rebased so it starts at position 0 ([0, totalLength - cut]).
    private (RoadSegmentDynamicAttributeValues<T> First, RoadSegmentDynamicAttributeValues<T> Second) SplitAtMeasure(
        RoadSegmentPositionV2 cutPosition, double totalLength)
    {
        var cut = cutPosition.ToDouble();
        totalLength = totalLength.RoundToCm();

        var firstItems = new List<RoadSegmentDynamicAttributeValue<T>>();
        var secondItems = new List<RoadSegmentDynamicAttributeValue<T>>();

        foreach (var value in Values)
        {
            var from = value.Coverage.From.ToDouble();
            var to = value.Coverage.To.ToDouble();

            var firstFrom = from;
            var firstTo = System.Math.Min(to, cut);
            if (firstTo > firstFrom)
            {
                firstItems.Add(new RoadSegmentDynamicAttributeValue<T>
                {
                    Coverage = new RoadSegmentPositionCoverage(new RoadSegmentPositionV2(firstFrom), new RoadSegmentPositionV2(firstTo)),
                    Side = value.Side,
                    Value = value.Value
                });
            }

            var secondFrom = System.Math.Max(from, cut) - cut;
            var secondTo = System.Math.Min(to, totalLength) - cut;
            if (secondTo > secondFrom)
            {
                secondItems.Add(new RoadSegmentDynamicAttributeValue<T>
                {
                    Coverage = new RoadSegmentPositionCoverage(new RoadSegmentPositionV2(secondFrom), new RoadSegmentPositionV2(secondTo)),
                    Side = value.Side,
                    Value = value.Value
                });
            }
        }

        return (new RoadSegmentDynamicAttributeValues<T>(firstItems), new RoadSegmentDynamicAttributeValues<T>(secondItems));
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
                    Side = RoadSegmentAttributeSide.Beide,
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
            if (a.Side != RoadSegmentAttributeSide.Beide) continue;

            for (var j = 0; j < items.Count; j++)
            {
                if (i == j) continue;

                var b = items[j];
                if (b.Side == RoadSegmentAttributeSide.Beide) continue;
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
        return (a == RoadSegmentAttributeSide.Links && b == RoadSegmentAttributeSide.Rechts)
               || (a == RoadSegmentAttributeSide.Rechts && b == RoadSegmentAttributeSide.Links);
    }
}
