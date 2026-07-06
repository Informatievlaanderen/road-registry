namespace RoadRegistry.Tests.AggregateTests.RoadSegment.SplitRoadSegment;

using System.Linq;
using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    private ScopedRoadNetwork BuildNetworkWithRealizedSegment()
    {
        return BuildNetworkWith(RoadSegment.Create(TestData.Segment1Added).WithoutChanges());
    }

    private ScopedRoadNetwork BuildNetworkWith(RoadSegment segment)
    {
        var startNode = RoadNode.Create(TestData.Segment1StartNodeAdded);
        var endNode = RoadNode.Create(TestData.Segment1EndNodeAdded);

        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            [startNode, endNode],
            [segment],
            [],
            []);
    }

    private static Point CutPosition(double x, double y)
    {
        return new Point(new Coordinate(x, y)) { SRID = WellknownSrids.Lambert08 };
    }

    private Point CutPositionAtMiddle()
    {
        return CutPosition(50.0, 50.0);
    }

    private static InMemoryRoadNetworkIdGenerator IdGenerator()
    {
        return new InMemoryRoadNetworkIdGenerator(initialValue: 100);
    }

    [Fact]
    public void WhenSplittingRealizedSegment_ThenOriginalIsHistorizedAndTwoNewSegmentsAreCreated()
    {
        // Arrange
        var roadNetwork = BuildNetworkWithRealizedSegment();
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;

        // Act
        var roadSegmentIds = roadNetwork.SplitRoadSegment(originalRoadSegmentId, CutPositionAtMiddle(), new InMemoryRoadNetworkIdGenerator(initialValue: 100), TestData.Provenance);

        // Assert
        roadNetwork.RoadSegments[originalRoadSegmentId].Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);

        var newSegments = roadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.RoadSegmentId != originalRoadSegmentId)
            .ToList();
        newSegments.Should().HaveCount(2);
        newSegments.Should().OnlyContain(x => x.Status == RoadSegmentStatusV2.Gerealiseerd);

        roadSegmentIds.Should().HaveCount(2);
        roadSegmentIds.Should().BeEquivalentTo(newSegments.Select(x => x.RoadSegmentId));

        // Situation 1 events: the original is retired-because-of-split and a split event without modifications.
        var originalChanges = roadNetwork.RoadSegments[originalRoadSegmentId].GetChanges();
        originalChanges.Should().ContainSingle(x => x is RoadSegmentWasRetiredBecauseOfSplit);
        var splitEvent = originalChanges.OfType<RoadSegmentWasSplit>().Single();
        splitEvent.Modifications.Should().BeNull();
        splitEvent.NewRoadSegmentIds.Should().BeEquivalentTo(newSegments.Select(x => x.RoadSegmentId));

        newSegments.Should().OnlyContain(x => x.GetChanges().OfType<RoadSegmentWasAdded>().Any());
    }

    [Fact]
    public void WhenOnePartIsMoreThan70Percent_ThenOriginalIsModifiedAndOneNewSegmentIsAdded()
    {
        // Arrange
        var roadNetwork = BuildNetworkWithRealizedSegment();
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;
        // The segment runs (0,0)->(50,50)->(100,100); cutting near the start leaves the larger part (>70%) with the original id.
        var cutNearStart = new Point(new Coordinate(10.0, 10.0)) { SRID = WellknownSrids.Lambert08 };

        // Act
        var roadSegmentIds = roadNetwork.SplitRoadSegment(originalRoadSegmentId, cutNearStart, new InMemoryRoadNetworkIdGenerator(initialValue: 100), TestData.Provenance);

        // Assert
        var original = roadNetwork.RoadSegments[originalRoadSegmentId];
        original.IsRemoved.Should().BeFalse();
        original.Status.Should().Be(RoadSegmentStatusV2.Gerealiseerd);

        var newSegments = roadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.RoadSegmentId != originalRoadSegmentId)
            .ToList();
        newSegments.Should().HaveCount(1);

        roadSegmentIds.Should().HaveCount(2);
        roadSegmentIds.Should().Contain(originalRoadSegmentId);
        roadSegmentIds.Should().Contain(newSegments.Single().RoadSegmentId);

        // Situation 2 events: no retire, a split event on the kept segment carrying the modifications.
        var originalChanges = original.GetChanges();
        originalChanges.Should().NotContain(x => x is RoadSegmentWasRetiredBecauseOfSplit);
        var splitEvent = originalChanges.OfType<RoadSegmentWasSplit>().Single();
        splitEvent.Modifications.Should().NotBeNull();
        splitEvent.NewRoadSegmentIds.Should().Contain(originalRoadSegmentId);
        splitEvent.NewRoadSegmentIds.Should().Contain(newSegments.Single().RoadSegmentId);
    }

    [Fact]
    public void WhenSplittingRealizedSegment_ThenGeometriesAreSplitAtTheCutPosition()
    {
        // Arrange
        var roadNetwork = BuildNetworkWithRealizedSegment();
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;

        // Act
        roadNetwork.SplitRoadSegment(originalRoadSegmentId, CutPositionAtMiddle(), new InMemoryRoadNetworkIdGenerator(initialValue: 100), TestData.Provenance);

        // Assert
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
        roadNetwork.SplitRoadSegment(originalRoadSegmentId, CutPositionAtMiddle(), new InMemoryRoadNetworkIdGenerator(initialValue: 100), TestData.Provenance);

        // Assert
        var cut = new Coordinate(50.0, 50.0);
        roadNetwork.GetNonRemovedRoadNodes()
            .Should().Contain(x => x.Geometry.Value.Coordinate.Equals2D(cut));
    }

    [Fact]
    public void WhenOnePartIsMoreThan70Percent_CuttingNearEnd_ThenOriginalKeepsLargestPart()
    {
        // Arrange
        var roadNetwork = BuildNetworkWithRealizedSegment();
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;
        // Cutting near the end leaves the larger part (start -> cut, >70%) with the original id.
        var cutNearEnd = CutPosition(90.0, 90.0);

        // Act
        var roadSegmentIds = roadNetwork.SplitRoadSegment(originalRoadSegmentId, cutNearEnd, IdGenerator(), TestData.Provenance);

        // Assert
        var original = roadNetwork.RoadSegments[originalRoadSegmentId];
        original.IsRemoved.Should().BeFalse();
        original.Status.Should().Be(RoadSegmentStatusV2.Gerealiseerd);

        var newSegments = roadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.RoadSegmentId != originalRoadSegmentId)
            .ToList();
        newSegments.Should().HaveCount(1);

        roadSegmentIds.Should().HaveCount(2);
        roadSegmentIds.Should().Contain(originalRoadSegmentId);

        var splitEvent = original.GetChanges().OfType<RoadSegmentWasSplit>().Single();
        splitEvent.Modifications.Should().NotBeNull();
    }

    [Fact]
    public void WhenSplittingPlannedSegment_ThenNoValidationNodeIsAddedAndPartsHaveNoNodes()
    {
        // Arrange
        var plannedSegment = RoadSegment
            .Create(TestData.Segment1Added with { Status = RoadSegmentStatusV2.Gepland, StartNodeId = null, EndNodeId = null })
            .WithoutChanges();
        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), [], [plannedSegment], [], []);
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;

        // Act
        roadNetwork.SplitRoadSegment(originalRoadSegmentId, CutPositionAtMiddle(), IdGenerator(), TestData.Provenance);

        // Assert
        roadNetwork.GetNonRemovedRoadNodes().Should().BeEmpty();

        var newSegments = roadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.RoadSegmentId != originalRoadSegmentId)
            .ToList();
        newSegments.Should().HaveCount(2);
        newSegments.Should().OnlyContain(x => x.Status == RoadSegmentStatusV2.Gepland);
        newSegments.Should().OnlyContain(x => x.StartNodeId == null && x.EndNodeId == null);
    }

    [Fact]
    public void WhenSplittingTooCloseToStartNode_ThenTooCloseProblem()
    {
        // Arrange
        var roadNetwork = BuildNetworkWithRealizedSegment();
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;
        var cutNearStart = CutPosition(0.5, 0.5);

        // Act
        var act = () => roadNetwork.SplitRoadSegment(originalRoadSegmentId, cutNearStart, IdGenerator(), TestData.Provenance);

        // Assert
        act.Should().Throw<RoadRegistryProblemsException>()
            .Which.Problems.Should().Contain(x => x.Reason == "RoadSegmentSplitPositionTooCloseToRoadNode");
    }

    [Fact]
    public void WhenSplittingTooCloseToEndNode_ThenTooCloseProblem()
    {
        // Arrange
        var roadNetwork = BuildNetworkWithRealizedSegment();
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;
        var cutNearEnd = CutPosition(99.5, 99.5);

        // Act
        var act = () => roadNetwork.SplitRoadSegment(originalRoadSegmentId, cutNearEnd, IdGenerator(), TestData.Provenance);

        // Assert
        act.Should().Throw<RoadRegistryProblemsException>()
            .Which.Problems.Should().Contain(x => x.Reason == "RoadSegmentSplitPositionTooCloseToRoadNode");
    }

    [Fact]
    public void WhenSplittingNonExistentSegment_ThenNotFoundProblem()
    {
        // Arrange
        var roadNetwork = BuildNetworkWithRealizedSegment();

        // Act
        var act = () => roadNetwork.SplitRoadSegment(new RoadSegmentId(999), CutPositionAtMiddle(), IdGenerator(), TestData.Provenance);

        // Assert
        act.Should().Throw<RoadRegistryProblemsException>()
            .Which.Problems.Should().Contain(x => x.Reason == "RoadSegmentSplitNotFound");
    }

    [Fact]
    public void WhenSplittingSegmentThatIsNotV2_ThenNotCompletedInwinningProblem()
    {
        // Arrange
        var notMigratedSegment = RoadSegment.CreateForMigration(
            TestData.Segment1Added.RoadSegmentId,
            TestData.Segment1Added.Geometry,
            RoadSegmentStatusV2.Gerealiseerd,
            new RoadNodeId(1),
            new RoadNodeId(2));
        var roadNetwork = BuildNetworkWith(notMigratedSegment);
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;

        // Act
        var act = () => roadNetwork.SplitRoadSegment(originalRoadSegmentId, CutPositionAtMiddle(), IdGenerator(), TestData.Provenance);

        // Assert
        act.Should().Throw<RoadRegistryProblemsException>()
            .Which.Problems.Should().Contain(x => x.Reason == "RoadSegmentSplitNotCompletedInwinning");
    }

    [Fact]
    public void WhenSplittingSegmentWithInvalidStatus_ThenStatusNotValidProblem()
    {
        // Arrange
        var segment = RoadSegment
            .Create(TestData.Segment1Added with { Status = RoadSegmentStatusV2.NietGerealiseerd })
            .WithoutChanges();
        var roadNetwork = BuildNetworkWith(segment);
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;

        // Act
        var act = () => roadNetwork.SplitRoadSegment(originalRoadSegmentId, CutPositionAtMiddle(), IdGenerator(), TestData.Provenance);

        // Assert
        act.Should().Throw<RoadRegistryProblemsException>()
            .Which.Problems.Should().Contain(x => x.Reason == "RoadSegmentSplitStatusNotValid");
    }

    [Fact]
    public void WhenCutPositionTooFarFromSegment_ThenTooFarProblem()
    {
        // Arrange
        var roadNetwork = BuildNetworkWithRealizedSegment();
        var originalRoadSegmentId = TestData.Segment1Added.RoadSegmentId;
        var cutFarAway = CutPosition(50.0, 60.0);

        // Act
        var act = () => roadNetwork.SplitRoadSegment(originalRoadSegmentId, cutFarAway, IdGenerator(), TestData.Provenance);

        // Assert
        act.Should().Throw<RoadRegistryProblemsException>()
            .Which.Problems.Should().Contain(x => x.Reason == "RoadSegmentSplitPositionTooFarFromRoadSegment");
    }
}
