namespace RoadRegistry.Tests.AggregateTests.RoadSegment.SplitRoadSegment;

using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using Xunit;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

// Splitting derives the new attribute positions from measures along the original line, but then rounds the two part
// geometries to the centimetre. Rounding moves the interpolated cut vertex, so a part can measure a different
// centimetre than the measure its positions came from - which leaves the trailing position one centimetre off its own
// geometry and makes every later change to that segment fail with ToPositionNotEqualToLength.
//
// Observed on road segment 818746: migrated at 85.51529 -> 85.52 (consistent), then split, after which the second part
// had a geometry of 76.88342 -> 76.88 while all of its attributes ended at 76.89.
public class SplitPositionConsistencyTests : AggregateTestBase
{
    private ScopedRoadNetwork BuildNetwork()
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            [RoadNode.Create(TestData.Segment1StartNodeAdded), RoadNode.Create(TestData.Segment1EndNodeAdded)],
            [RoadSegment.Create(TestData.Segment1Added).WithoutChanges()],
            [],
            []);
    }

    private static Point CutAt(double x, double y)
    {
        return new Point(new Coordinate(x, y)) { SRID = WellknownSrids.Lambert08 };
    }

    private List<(RoadSegmentId Id, double GeometryLength, double TrailingPosition)> SplitAndMeasure(Point cutPosition)
    {
        var roadNetwork = BuildNetwork();
        var originalId = TestData.Segment1Added.RoadSegmentId;

        roadNetwork.SplitRoadSegment(originalId, cutPosition, new InMemoryRoadNetworkIdGenerator(initialValue: 100), TestData.Provenance);

        return roadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.RoadSegmentId != originalId)
            .Select(x => (
                x.RoadSegmentId,
                GeometryLength: x.Geometry.Value.Length.RoundToCm(),
                TrailingPosition: x.Attributes!.Morphology.Values.Max(v => v.Coverage.To).ToDouble()))
            .ToList();
    }

    [Fact]
    public void EveryCutPosition_LeavesBothPartsWithPositionsMatchingTheirOwnGeometry()
    {
        // The segment runs (0,0) - (50,50) - (100,100), so the length is irrational and every cut lands on a
        // coordinate that has to be rounded. Sweeping the cut across the whole segment catches the positions where
        // the measure and the rounded geometry disagree, instead of relying on one hand-picked spot.
        var mismatches = new List<string>();

        // Kept clear of both ends: a cut within a metre of a road node is rejected outright.
        for (var step = 3; step <= 196; step++)
        {
            var offset = step * 0.5;
            foreach (var part in SplitAndMeasure(CutAt(offset, offset)))
            {
                if (part.GeometryLength != part.TrailingPosition)
                {
                    mismatches.Add($"cut at ({offset}, {offset}) part {part.Id}: geometry {part.GeometryLength} vs position {part.TrailingPosition}");
                }
            }
        }

        mismatches.Should().BeEmpty();
    }

    [Fact]
    public void SplittingByJunction_LeavesBothPartsWithPositionsMatchingTheirOwnGeometry()
    {
        // Same derivation, same rounding, in ScopedRoadNetwork-SplitRoadSegmentsByJunction.
        var roadNetwork = BuildNetwork();
        var originalId = TestData.Segment1Added.RoadSegmentId;

        roadNetwork.SplitRoadSegment(originalId, CutAt(30.5, 30.5), new InMemoryRoadNetworkIdGenerator(initialValue: 100), TestData.Provenance);

        foreach (var segment in roadNetwork.GetNonRemovedRoadSegments().Where(x => x.RoadSegmentId != originalId))
        {
            segment.Attributes!.Morphology.Values.Max(v => v.Coverage.To).ToDouble()
                .Should().Be(segment.Geometry.Value.Length.RoundToCm(),
                    $"segment {segment.RoadSegmentId} must carry positions that match its own geometry");
        }
    }
}
