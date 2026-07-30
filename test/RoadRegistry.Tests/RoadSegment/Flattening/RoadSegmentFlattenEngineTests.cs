namespace RoadRegistry.Tests.RoadSegment.Flattening;

using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using RoadRegistry.RoadSegment.Flattening;
using RoadRegistry.RoadSegment.ValueObjects;
using static RoadRegistry.RoadSegment.Flattening.RoadSegmentFlattenEngine;

// Locks the behaviour of the shared flatten engine that the extract, WmsWfsV2 and PBS derived-wegsegment flatteners
// all build on: trailing-coverage normalization to the geometry length and side-aware resolution. The scenario tests
// below mirror the extract RoadSegmentFlattenerTests cases at the engine level: which set of attribute coverages/sides
// yields which sub-ranges with which resolved values, including attributes that change at different positions.
public class RoadSegmentFlattenEngineTests
{
    private static int Links => RoadSegmentAttributeSide.Links.Translation.Identifier;
    private static int Rechts => RoadSegmentAttributeSide.Rechts.Translation.Identifier;
    private static int Beide => RoadSegmentAttributeSide.Beide.Translation.Identifier;

    private sealed record Row(double From, double To, int Side, string Value);

    private static List<Cover<Row>> Track(double length, params Row[] rows)
    {
        return Normalize(rows, length, r => r.From, r => r.To);
    }

    private static string? Value(List<Cover<Row>> track, FlatRange range)
    {
        return Resolve(track, range.From, range.To)?.Value;
    }

    private static string? SideValue(List<Cover<Row>> track, FlatRange range, int side)
    {
        return ResolveSided(track, range.From, range.To, side, r => r.Side)?.Value;
    }

    [Fact]
    public void Normalize_SnapsTrailingCoverageShorterThanLength_ToLength()
    {
        var rows = new[] { new Row(0, 50, Beide, "a"), new Row(50, 90, Beide, "b") };

        var cover = Normalize(rows, 100, r => r.From, r => r.To);

        cover.Select(c => c.To).Should().Equal(50, 100); // the trailing 90 is snapped to the geometry length 100
    }

    [Fact]
    public void Normalize_ClampsTrailingCoverageLongerThanLength_ToLength()
    {
        var rows = new[] { new Row(0, 120, Beide, "a") };

        var cover = Normalize(rows, 100, r => r.From, r => r.To);

        cover.Single().To.Should().Be(100);
    }

    [Fact]
    public void Ranges_SplitsAtEveryBoundary_AndClampsTrailingExtentToLength()
    {
        var a = Normalize(new[] { new Row(0, 20, Beide, "1"), new Row(20, 95, Beide, "2") }, 100, r => r.From, r => r.To);

        var ranges = Ranges(100, a.Pairs());

        ranges.Select(r => (r.From, r.To)).Should().Equal((0d, 20d), (20d, 100d));
        ranges.Last().ToActual.Should().Be(100); // trailing geometry extent clamped to the length
    }

    [Fact]
    public void ResolveSided_PrefersBeideTail_OverShorterSideSpecificCoverage()
    {
        // A side-specific coverage that stops halfway must not be extended past a Beide coverage that already tiles the
        // rest of the segment: resolving the left side beyond the split must yield the Beide value, not the left one.
        var rows = new[]
        {
            new Row(0, 50, Links, "L"),
            new Row(0, 50, Rechts, "R"),
            new Row(50, 100, Beide, "B")
        };
        var cover = Normalize(rows, 100, r => r.From, r => r.To);

        ResolveSided(cover, 0, 50, Links, r => r.Side)!.Value.Should().Be("L");
        ResolveSided(cover, 50, 100, Links, r => r.Side)!.Value.Should().Be("B");
        ResolveSided(cover, 50, 100, Rechts, r => r.Side)!.Value.Should().Be("B");
    }

    [Fact]
    public void SingleFullCoverage_YieldsOneRangeWithThatValue()
    {
        var access = Track(100, new Row(0, 100, Beide, "1"));

        var ranges = Ranges(100, access.Pairs());

        ranges.Should().ContainSingle();
        Value(access, ranges.Single()).Should().Be("1");
    }

    [Fact]
    public void AttributesChangingAtDifferentPositions_YieldTheUnionOfSplitsWithResolvedValues()
    {
        // Access changes at 20, category at 50: overlapping ranges but not on the same positions, so the union splits
        // the segment into three sub-ranges, each resolving both attributes independently.
        var access = Track(100, new Row(0, 20, Beide, "1"), new Row(20, 100, Beide, "2"));
        var category = Track(100, new Row(0, 50, Beide, "a"), new Row(50, 100, Beide, "b"));

        var ranges = Ranges(100, access.Pairs(), category.Pairs());

        ranges.Select(r => (r.From, r.To)).Should().Equal((0d, 20d), (20d, 50d), (50d, 100d));
        ranges.Select(r => (Value(access, r), Value(category, r)))
            .Should().Equal(("1", "a"), ("2", "a"), ("2", "b"));
    }

    [Fact]
    public void LeftRightAndBeideCoverages_ResolvePerSideAcrossRanges()
    {
        // The left/right street names differ over [0,50] and then collapse to a single Beide value over [50,100],
        // while access changes at 20. Each range resolves access plus the correct per-side value.
        var access = Track(100, new Row(0, 20, Beide, "1"), new Row(20, 100, Beide, "2"));
        var street = Track(100, new Row(0, 50, Links, "L"), new Row(0, 50, Rechts, "R"), new Row(50, 100, Beide, "B"));

        var ranges = Ranges(100, access.Pairs(), street.Pairs());

        ranges.Select(r => (r.From, r.To)).Should().Equal((0d, 20d), (20d, 50d), (50d, 100d));
        ranges.Select(r => (Value(access, r), SideValue(street, r, Links), SideValue(street, r, Rechts)))
            .Should().Equal(("1", "L", "R"), ("2", "L", "R"), ("2", "B", "B"));
    }

    [Fact]
    public void TrailingCoverageShortOfLength_LastRangeStillExtendsToLengthAndKeepsItsValue()
    {
        // The last coverage stops at 28.81 while the geometry is 28.8123 long: the trailing range must extend to the
        // actual length and still resolve the last attribute value.
        var access = Track(28.8123, new Row(0, 20, Beide, "1"), new Row(20, 28.81, Beide, "2"));

        var ranges = Ranges(28.8123, access.Pairs());

        ranges.Should().HaveCount(2);
        ranges.Last().From.Should().Be(20);
        ranges.Last().ToActual.Should().Be(28.8123);
        Value(access, ranges.Last()).Should().Be("2");
    }
}
