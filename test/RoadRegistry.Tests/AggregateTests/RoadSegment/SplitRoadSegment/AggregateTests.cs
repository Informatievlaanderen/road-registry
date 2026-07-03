namespace RoadRegistry.Tests.AggregateTests.RoadSegment.SplitRoadSegment;

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
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    private ScopedRoadNetwork BuildNetworkWithRealizedSegment()
    {
        var startNode = RoadNode.Create(TestData.Segment1StartNodeAdded);
        var endNode = RoadNode.Create(TestData.Segment1EndNodeAdded);
        var segment = RoadSegment.Create(TestData.Segment1Added).WithoutChanges();

        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            [startNode, endNode],
            [segment],
            [],
            []);
    }

    private Point CutPositionAtMiddle()
    {
        return new Point(new Coordinate(50.0, 50.0)) { SRID = WellknownSrids.Lambert08 };
    }

    [Fact]
    public void WhenSplittingRealizedSegment_ThenOriginalIsHistorizedAndTwoNewSegmentsAreCreated()
    {
        // Arrange
        var roadNetwork = BuildNetworkWithRealizedSegment();
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;

        // Act
        var problems = roadNetwork.SplitRoadSegment(originalRoadSegmentId, CutPositionAtMiddle(), new InMemoryRoadNetworkIdGenerator(initialValue: 100), TestData.Provenance);

        problems.Should().HaveNoError();

        roadNetwork.RoadSegments[originalRoadSegmentId].Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);

        var newSegments = roadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.RoadSegmentId != originalRoadSegmentId)
            .ToList();
        newSegments.Should().HaveCount(2);
        newSegments.Should().OnlyContain(x => x.Status == RoadSegmentStatusV2.Gerealiseerd);
    }

    [Fact]
    public void WhenSplittingRealizedSegment_ThenGeometriesAreSplitAtTheCutPosition()
    {
        // Arrange
        var roadNetwork = BuildNetworkWithRealizedSegment();
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;

        // Act
        var problems = roadNetwork.SplitRoadSegment(originalRoadSegmentId, CutPositionAtMiddle(), new InMemoryRoadNetworkIdGenerator(initialValue: 100), TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();

        var newSegments = roadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.RoadSegmentId != originalRoadSegmentId)
            .ToList();

        var coordinateSets = newSegments
            .Select(x => x.Geometry.Value.GetSingleLineString().Coordinates)
            .ToList();

        // Every new part starts or ends at the cut position (50, 50)
        var cut = new Coordinate(50.0, 50.0);
        coordinateSets.Should().OnlyContain(coords => coords.First().Equals2D(cut) || coords.Last().Equals2D(cut));

        // Together the two parts still cover the original start (0,0) and end (100,100)
        var allEndpoints = coordinateSets.SelectMany(coords => new[] { coords.First(), coords.Last() }).ToList();
        allEndpoints.Should().Contain(c => c.Equals2D(new Coordinate(0.0, 0.0)));
        allEndpoints.Should().Contain(c => c.Equals2D(new Coordinate(100.0, 100.0)));
    }

    [Fact]
    public void WhenSplittingRealizedSegment_ThenAValidationNodeIsAddedAtTheCutPosition()
    {
        // Arrange
        var roadNetwork = BuildNetworkWithRealizedSegment();
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;

        // Act
        var problems = roadNetwork.SplitRoadSegment(originalRoadSegmentId, CutPositionAtMiddle(), new InMemoryRoadNetworkIdGenerator(initialValue: 100), TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();

        var cut = new Coordinate(50.0, 50.0);
        roadNetwork.GetNonRemovedRoadNodes()
            .Should().Contain(x => x.Geometry.Value.Coordinate.Equals2D(cut));
    }

    [Fact]
    public void WhenSplittingTooCloseToStartNode_ThenTooCloseProblem()
    {
        // Arrange
        var roadNetwork = BuildNetworkWithRealizedSegment();
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;
        var cutNearStart = new Point(new Coordinate(0.5, 0.5)) { SRID = WellknownSrids.Lambert08 };

        // Act
        var problems = roadNetwork.SplitRoadSegment(originalRoadSegmentId, cutNearStart, new InMemoryRoadNetworkIdGenerator(initialValue: 100), TestData.Provenance);

        // Assert
        problems.Should().Contain(x => x.Reason == "RoadSegmentSplitPositionTooCloseToRoadNode");
    }
}
