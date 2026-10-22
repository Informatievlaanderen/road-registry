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
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;
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
        bool withJunction = true,
        bool segment1Migrated = true)
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
        var segment1 = segment1Migrated
            ? RoadSegment.Create(segment1Added).WithoutChanges()
            : RoadSegment.CreateForMigration(segment1Added.RoadSegmentId, segment1Added.Geometry, segment1Added.Status, segment1Added.StartNodeId, segment1Added.EndNodeId);

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
        roadNetwork.SplitRoadSegmentsByJunction(Segment1Id, Segment2Id, IdGenerator(), TestData.Provenance);

        // Assert: the two originals are historized (both cut halfway, so neither keeps its identifier).
        roadNetwork.RoadSegments[Segment1Id].Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);
        roadNetwork.RoadSegments[Segment2Id].Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);

        var newSegments = roadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.RoadSegmentId != Segment1Id && x.RoadSegmentId != Segment2Id)
            .ToList();
        newSegments.Should().HaveCount(4);
        newSegments.Should().OnlyContain(x => x.Status == RoadSegmentStatusV2.Gerealiseerd);

        roadNetwork.RoadSegments[Segment1Id].GetChanges().Should().Contain(x => x is RoadSegmentWasRetiredBecauseOfSplit);
        roadNetwork.RoadSegments[Segment2Id].GetChanges().Should().Contain(x => x is RoadSegmentWasRetiredBecauseOfSplit);

        // Historizing the originals is a status change: they are reported as modified, never as removed.
        roadNetwork.SummaryOfLastChange!.RoadSegments.Modified.Should().BeEquivalentTo([Segment1Id, Segment2Id]);
        roadNetwork.SummaryOfLastChange.RoadSegments.Added.Should().BeEquivalentTo(newSegments.Select(x => x.RoadSegmentId));
        roadNetwork.SummaryOfLastChange.RoadSegments.Removed.Should().BeEmpty();
    }

    [Fact]
    public void WhenOneSegmentPartIsLongEnough_ThenItKeepsItsIdentifierAndOnlyOnePartIsAdded()
    {
        // Arrange: the segments cross at (90,90), near Segment1's end node. Segment1's larger part (from its start to the
        // crossing) is >70% of its length, so Segment1 keeps its identifier and only its short part is added as a new
        // segment. Segment2 is cut at its midpoint (50/50), so it is historized and two new segments are added.
        var roadNetwork = BuildCrossingNetwork(new Coordinate(90.0, 90.0));

        // Act
        roadNetwork.SplitRoadSegmentsByJunction(Segment1Id, Segment2Id, IdGenerator(), TestData.Provenance);

        // Assert: Segment1 keeps its identifier (modified in place, not historized).
        var segment1 = roadNetwork.RoadSegments[Segment1Id];
        segment1.IsRemoved.Should().BeFalse();
        segment1.Status.Should().Be(RoadSegmentStatusV2.Gerealiseerd);
        segment1.GetChanges().Should().NotContain(x => x is RoadSegmentWasRetiredBecauseOfSplit);

        // Segment2 does not keep its identifier: it is historized.
        roadNetwork.RoadSegments[Segment2Id].Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);
        roadNetwork.RoadSegments[Segment2Id].GetChanges().Should().Contain(x => x is RoadSegmentWasRetiredBecauseOfSplit);

        // Three new segments: one from Segment1's kept split, two from Segment2's historized split.
        var newSegments = roadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.RoadSegmentId != Segment1Id && x.RoadSegmentId != Segment2Id)
            .ToList();
        newSegments.Should().HaveCount(3);
        newSegments.Should().OnlyContain(x => x.Status == RoadSegmentStatusV2.Gerealiseerd);
    }

    [Fact]
    public void WhenSplittingTwoCrossingRealizedSegments_ThenARealNodeIsAddedAtTheCrossingAndTheJunctionIsRemoved()
    {
        // Arrange
        var roadNetwork = BuildCrossingNetwork(Crossing);

        // Act
        roadNetwork.SplitRoadSegmentsByJunction(Segment1Id, Segment2Id, IdGenerator(), TestData.Provenance);

        // Assert: a real road node (grensknoop=false, echte knoop) exists at the crossing.
        var crossingNode = roadNetwork.GetNonRemovedRoadNodes()
            .SingleOrDefault(x => x.Geometry.Value.Coordinate.Equals2D(Crossing));
        crossingNode.Should().NotBeNull();
        crossingNode!.Grensknoop.Should().BeFalse();
        crossingNode.Type.Should().Be(RoadNodeTypeV2.EchteKnoop);

        var addedEvent = crossingNode.GetChanges().OfType<RoadRegistry.RoadNode.Events.V2.RoadNodeWasAdded>().Single();
        addedEvent.Type.Should().Be(RoadNodeTypeV2.EchteKnoop);

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
    public void WhenOneSegmentHasNotCompletedInwinning_ThenNotCompletedInwinningProblem()
    {
        // Arrange: segment1 has not been migrated to V2 yet.
        var roadNetwork = BuildCrossingNetwork(Crossing, segment1Migrated: false);

        // Act
        var act = () => roadNetwork.SplitRoadSegmentsByJunction(Segment1Id, Segment2Id, IdGenerator(), TestData.Provenance);

        // Assert
        act.Should().Throw<RoadRegistryProblemsException>()
            .Which.Problems.Should().Contain(x => x.Reason == "RoadSegmentNotCompletedInwinning");
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
            .Which.Problems.Should().Contain(x => x.Reason == "RoadSegmentSplitPositionTooCloseToStartVertex");
    }

    [Fact]
    public void WhenTheCrossingLiesCloseToAnExistingVertex_ThenTheSplitSucceeds()
    {
        // Arrange: road segments 713450 (Coolhemstraat) and 654324 (Rijksweg) as they are on staging, in Lambert 2008,
        // with grade-separated junction 2941 between them. The crossing lies about 2cm from vertex 645441.20 695958.69
        // of 654324.
        var segment1Id = new RoadSegmentId(713450);
        var segment2Id = new RoadSegmentId(654324);

        var segment1Geometry = LineFromPosList(
            "645417.80 695951.54 645420.96 695952.50 645421.96 695952.81 645589.80 696004.29 645594.94 696006.26 645599.09 696008.44 645605.00 696011.86");
        var segment2Geometry = LineFromPosList(
            "646254.49 695043.00 646090.38 695189.68 646076.28 695202.25 646025.19 695247.80 646019.76 695252.63 646011.03 695260.41 645996.73 695273.22 645937.47 695326.24 645920.56 695340.72 645902.14 695357.02 645873.29 695383.86 645837.82 695417.45 645774.97 695481.16 645691.43 695577.60 645674.44 695598.69 645655.36 695622.49 645619.80 695673.50 645594.16 695710.27 645575.39 695740.09 645545.13 695789.37 645539.03 695799.30 645533.91 695807.65 645522.66 695826.00 645455.20 695935.89 645441.20 695958.69 645426.76 695982.22 645408.84 696011.42 645399.56 696027.64 645365.38 696081.21 645327.44 696133.33 645293.84 696173.88 645244.60 696226.59 645230.36 696240.58 645206.01 696262.58 645205.89 696262.69 645187.59 696279.24 645183.76 696282.77 645140.60 696316.64 645071.38 696371.66 644974.63 696447.82 644896.28 696509.36");

        var node1 = NodeAt(1285200, segment1Geometry.Value.Coordinates[0]);
        var node2 = NodeAt(1426900, segment1Geometry.Value.Coordinates[^1]);
        var node3 = NodeAt(1426567, segment2Geometry.Value.Coordinates[0]);
        var node4 = NodeAt(2088826, segment2Geometry.Value.Coordinates[^1]);

        var gradeSeparatedJunction = GradeSeparatedJunction.Create(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(2941),
            LowerRoadSegmentId = segment1Id,
            UpperRoadSegmentId = segment2Id,
            Type = GradeSeparatedJunctionTypeV2.Brug,
            Geometry = JunctionGeometry.Create(Point(645441.19, 695958.71)),
            Provenance = new ProvenanceData(TestData.Provenance)
        }).WithoutChanges();

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            [node1, node2, node3, node4],
            [
                RoadSegment.Create(SegmentAlong(segment1Id, node1, node2, segment1Geometry)).WithoutChanges(),
                RoadSegment.Create(SegmentAlong(segment2Id, node3, node4, segment2Geometry)).WithoutChanges()
            ],
            [gradeSeparatedJunction],
            []);

        // Act
        roadNetwork.SplitRoadSegmentsByJunction(segment1Id, segment2Id, IdGenerator(), TestData.Provenance);

        // Assert: the vertex next to the crossing is dropped from both parts of 654324, which still meet on the crossing.
        var crossing = new Coordinate(645441.19, 695958.71);
        var newSegments = roadNetwork.GetNonRemovedRoadSegments()
            .Where(x => x.RoadSegmentId != segment1Id && x.RoadSegmentId != segment2Id)
            .ToList();
        newSegments.Should().HaveCount(3);

        var parts = newSegments.Select(x => x.Geometry.Value.GetSingleLineString()).ToList();
        parts.Should().OnlyContain(x => x.StartPoint.Coordinate.Equals2D(crossing) || x.EndPoint.Coordinate.Equals2D(crossing));
        parts.SelectMany(x => x.Coordinates.Zip(x.Coordinates.Skip(1), (a, b) => a.Distance(b)))
            .Should().OnlyContain(x => x >= Distances.MinimumDistanceBetweenVertices);
        parts.Should().NotContain(x => x.Coordinates.Any(c => c.Equals2D(new Coordinate(645441.20, 695958.69))));

        roadNetwork.GradeSeparatedJunctions.Values.Should().OnlyContain(x => x.IsRemoved);
    }

    private static RoadSegmentGeometry LineFromPosList(string posList)
    {
        var ordinates = posList.Split(' ').Select(x => double.Parse(x, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
        var coordinates = Enumerable.Range(0, ordinates.Length / 2)
            .Select(i => new Coordinate(ordinates[i * 2], ordinates[i * 2 + 1]))
            .ToArray();

        return LineThrough(coordinates).ToRoadSegmentGeometry();
    }

    private RoadNode NodeAt(int roadNodeId, Coordinate coordinate)
    {
        return RoadNode.Create(new RoadNodeWasAdded
        {
            RoadNodeId = new RoadNodeId(roadNodeId),
            Geometry = Point(coordinate.X, coordinate.Y).ToRoadNodeGeometry(),
            Grensknoop = false,
            Provenance = new ProvenanceData(TestData.Provenance)
        }).WithoutChanges();
    }

    // Every attribute value covers the whole segment, so the segment is consistent with its own geometry.
    private RoadSegmentWasAdded SegmentAlong(RoadSegmentId roadSegmentId, RoadNode startNode, RoadNode endNode, RoadSegmentGeometry geometry)
    {
        var template = TestData.Segment1Added;

        return template with
        {
            RoadSegmentId = roadSegmentId,
            StartNodeId = startNode.RoadNodeId,
            EndNodeId = endNode.RoadNodeId,
            Geometry = geometry,
            Status = RoadSegmentStatusV2.Gerealiseerd,
            AccessRestriction = Spanning(template.AccessRestriction, geometry),
            Category = Spanning(template.Category, geometry),
            Morphology = Spanning(template.Morphology, geometry),
            StreetNameId = Spanning(template.StreetNameId, geometry),
            MaintenanceAuthorityId = Spanning(template.MaintenanceAuthorityId, geometry),
            SurfaceType = Spanning(template.SurfaceType, geometry),
            CarTrafficDirection = Spanning(template.CarTrafficDirection, geometry),
            BikeTrafficDirection = Spanning(template.BikeTrafficDirection, geometry),
            PedestrianTrafficDirection = Spanning(template.PedestrianTrafficDirection, geometry),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };
    }

    private static RoadSegmentDynamicAttributeValues<T> Spanning<T>(RoadSegmentDynamicAttributeValues<T> template, RoadSegmentGeometry geometry)
        where T : notnull
    {
        return new RoadSegmentDynamicAttributeValues<T>().Add(template.Values.First().Value, geometry);
    }
}
