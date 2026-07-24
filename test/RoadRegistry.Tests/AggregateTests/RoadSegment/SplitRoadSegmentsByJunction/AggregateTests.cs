namespace RoadRegistry.Tests.AggregateTests.RoadSegment.SplitRoadSegmentsByJunction;

using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using FluentAssertions;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using ProvenanceData = Be.Vlaanderen.Basisregisters.GrAr.Provenance.ProvenanceData;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    // Segment1 runs (0,0)->(50,50)->(100,100); the crossing Segment2 runs (100,0)->(50,50)->(0,100). They cross at (50,50).
    private static readonly Coordinate Crossing = new(50.0, 50.0);

    private RoadSegmentId Segment1Id => TestData.Segment1Added.RoadSegmentId;
    private RoadSegmentId Segment2Id => new(TestData.Segment1Added.RoadSegmentId.ToInt32() + 1);

    private static InMemoryRoadNetworkIdGenerator IdGenerator()
    {
        return new InMemoryRoadNetworkIdGenerator(initialValue: 100);
    }

    private static Point Point(double x, double y)
    {
        return new Point(new Coordinate(x, y)) { SRID = WellknownSrids.Lambert08 };
    }

    private static MultiLineString LineThrough(params Coordinate[] coordinates)
    {
        return new MultiLineString([new LineString(new CoordinateArraySequence(coordinates), GeometryConfiguration.GeometryFactory)])
        {
            SRID = WellknownSrids.Lambert08
        };
    }

    private ScopedRoadNetwork BuildCrossingNetwork(
        Coordinate crossing,
        RoadSegmentStatusV2? segment1Status = null,
        RoadSegmentStatusV2? segment2Status = null,
        bool withJunction = true)
    {
        // Segment2 crosses Segment1 at the crossing point, perpendicular to it (direction (1,-1)), with each half 70.71m
        // long, so its total length stays 141.42m (matching the dynamic-attribute coverage reused from Segment1).
        var node3Coord = new Coordinate(crossing.X - 50.0, crossing.Y + 50.0);
        var node4Coord = new Coordinate(crossing.X + 50.0, crossing.Y - 50.0);

        var node1 = RoadNode.Create(TestData.Segment1StartNodeAdded).WithoutChanges(); // (0,0)
        var node2 = RoadNode.Create(TestData.Segment1EndNodeAdded).WithoutChanges(); // (100,100)
        var node3 = RoadNode.Create(TestData.Segment2StartNodeAdded with { Geometry = Point(node3Coord.X, node3Coord.Y).ToRoadNodeGeometry() }).WithoutChanges();
        var node4 = RoadNode.Create(TestData.Segment2EndNodeAdded with { Geometry = Point(node4Coord.X, node4Coord.Y).ToRoadNodeGeometry() }).WithoutChanges();

        var segment1Added = TestData.Segment1Added with { Status = segment1Status ?? TestData.Segment1Added.Status };
        var segment1 = RoadSegment.Create(segment1Added).WithoutChanges();

        var segment2Added = TestData.Segment1Added with
        {
            RoadSegmentId = Segment2Id,
            Status = segment2Status ?? TestData.Segment1Added.Status,
            Geometry = LineThrough(node3Coord, crossing, node4Coord).ToRoadSegmentGeometry(),
            StartNodeId = TestData.Segment2StartNodeAdded.RoadNodeId,
            EndNodeId = TestData.Segment2EndNodeAdded.RoadNodeId
        };
        var segment2 = RoadSegment.Create(segment2Added).WithoutChanges();

        GradeJunction[] gradeJunctions = withJunction
            ?
            [
                GradeJunction.Create(new GradeJunctionWasAdded
                {
                    GradeJunctionId = new GradeJunctionId(1),
                    RoadSegmentId1 = Segment1Id,
                    RoadSegmentId2 = Segment2Id,
                    Geometry = JunctionGeometry.Create(new Point(crossing) { SRID = WellknownSrids.Lambert08 }),
                    Provenance = new ProvenanceData(TestData.Provenance)
                }).WithoutChanges()
            ]
            : [];

        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            [node1, node2, node3, node4],
            [segment1, segment2],
            [],
            gradeJunctions);
    }

    [Fact]
    public void WhenSplittingTwoCrossingRealizedSegments_ThenBothAreHistorizedAndFourNewSegmentsAreCreated()
    {
        // Arrange
        var roadNetwork = BuildCrossingNetwork(Crossing);

        // Act
        var roadSegmentIds = roadNetwork.SplitRoadSegmentsByJunction(Segment1Id, Segment2Id, IdGenerator(), TestData.Provenance);

        // Assert: the two originals are historized (both cut halfway, so neither keeps its identifier).
        roadNetwork.RoadSegments[Segment1Id].Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);
        roadNetwork.RoadSegments[Segment2Id].Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);

        var newSegments = roadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.RoadSegmentId != Segment1Id && x.RoadSegmentId != Segment2Id)
            .ToList();
        newSegments.Should().HaveCount(4);
        newSegments.Should().OnlyContain(x => x.Status == RoadSegmentStatusV2.Gerealiseerd);

        roadSegmentIds.Should().HaveCount(4);
        roadSegmentIds.Should().BeEquivalentTo(newSegments.Select(x => x.RoadSegmentId));

        roadNetwork.RoadSegments[Segment1Id].GetChanges().Should().Contain(x => x is RoadSegmentWasRetiredBecauseOfSplit);
        roadNetwork.RoadSegments[Segment2Id].GetChanges().Should().Contain(x => x is RoadSegmentWasRetiredBecauseOfSplit);
    }

    [Fact]
    public void WhenSplittingTwoCrossingRealizedSegments_ThenARealNodeIsAddedAtTheCrossingAndTheJunctionIsRemoved()
    {
        // Arrange
        var roadNetwork = BuildCrossingNetwork(Crossing);

        // Act
        roadNetwork.SplitRoadSegmentsByJunction(Segment1Id, Segment2Id, IdGenerator(), TestData.Provenance);

        // Assert: a real road node (grensknoop=false) exists at the crossing.
        var crossingNode = roadNetwork.GetNonRemovedRoadNodes()
            .SingleOrDefault(x => x.Geometry.Value.Coordinate.Equals2D(Crossing));
        crossingNode.Should().NotBeNull();
        crossingNode!.Grensknoop.Should().BeFalse();

        // The grade junction on the crossing is removed.
        roadNetwork.GradeJunctions.Values.Should().OnlyContain(x => x.IsRemoved);
    }

    [Fact]
    public void WhenNoJunctionBetweenTheSegments_ThenNoJunctionProblem()
    {
        // Arrange
        var roadNetwork = BuildCrossingNetwork(Crossing, withJunction: false);

        // Act
        var act = () => roadNetwork.SplitRoadSegmentsByJunction(Segment1Id, Segment2Id, IdGenerator(), TestData.Provenance);

        // Assert
        act.Should().Throw<RoadRegistryProblemsException>()
            .Which.Problems.Should().Contain(x => x.Reason == "RoadSegmentsSplitByJunctionNoJunctionBetweenRoadSegments");
    }

    [Fact]
    public void WhenOneSegmentDoesNotHaveStatusGerealiseerd_ThenStatusNotValidProblem()
    {
        // Arrange
        var roadNetwork = BuildCrossingNetwork(Crossing, segment2Status: RoadSegmentStatusV2.Gepland);

        // Act
        var act = () => roadNetwork.SplitRoadSegmentsByJunction(Segment1Id, Segment2Id, IdGenerator(), TestData.Provenance);

        // Assert
        act.Should().Throw<RoadRegistryProblemsException>()
            .Which.Problems.Should().Contain(x => x.Reason == "RoadSegmentsSplitByJunctionStatusNotValid");
    }

    [Fact]
    public void WhenOneSegmentDoesNotExist_ThenNotFoundProblem()
    {
        // Arrange
        var roadNetwork = BuildCrossingNetwork(Crossing);

        // Act
        var act = () => roadNetwork.SplitRoadSegmentsByJunction(Segment1Id, new RoadSegmentId(999999), IdGenerator(), TestData.Provenance);

        // Assert
        act.Should().Throw<RoadRegistryProblemsException>()
            .Which.Problems.Should().Contain(x => x.Reason == "RoadSegmentsSplitByJunctionRoadSegmentNotFound");
    }

    [Fact]
    public void WhenTheCrossingIsTooCloseToARoadNode_ThenTooCloseProblem()
    {
        // Arrange: the segments cross at (0.5,0.5), less than 1m along Segment1 from its start node.
        var nearStartCrossing = new Coordinate(0.5, 0.5);
        var roadNetwork = BuildCrossingNetwork(nearStartCrossing);

        // Act
        var act = () => roadNetwork.SplitRoadSegmentsByJunction(Segment1Id, Segment2Id, IdGenerator(), TestData.Provenance);

        // Assert
        act.Should().Throw<RoadRegistryProblemsException>()
            .Which.Problems.Should().Contain(x => x.Reason == "RoadSegmentSplitPositionTooCloseToRoadNode");
    }
}
