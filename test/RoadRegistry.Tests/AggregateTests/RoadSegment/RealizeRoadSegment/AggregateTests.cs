namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RealizeRoadSegment;

using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadRegistry.ValueObjects.ProblemCodes;
using RoadRegistry.ValueObjects.Problems;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    // The segment being realized. It is drawn but not knotted in, so it carries no road nodes of its own.
    private const int PlannedSegmentId = 1;

    // An existing realized road, running west to east along y = 0, split at (100, 0) so there is a node to connect to.
    private const int RealizedSegmentId = 2;
    private const int SecondRealizedSegmentId = 3;

    private RoadNodeWasAdded BuildNode(int id, double x, double y, RoadNodeTypeV2? type = null)
    {
        return new RoadNodeWasAdded
        {
            RoadNodeId = new RoadNodeId(id),
            Geometry = new Point(new Coordinate(x, y)) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry(),
            Grensknoop = false,
            Type = type,
            Provenance = new ProvenanceData(TestData.Provenance)
        };
    }

    private RoadSegmentGeometry BuildGeometry(params (double X, double Y)[] coordinates)
    {
        return new MultiLineString([new LineString(coordinates.Select(x => new Coordinate(x.X, x.Y)).ToArray())])
            .WithSrid(WellknownSrids.Lambert08)
            .ToRoadSegmentGeometry();
    }

    private static RoadSegmentDynamicAttributeValues<T> Spanning<T>(RoadSegmentDynamicAttributeValues<T> template, RoadSegmentGeometry geometry)
        where T : notnull
    {
        return new RoadSegmentDynamicAttributeValues<T>().Add(template.Values.First().Value, geometry);
    }

    // Every attribute value covers the whole segment, so the segment is internally consistent with its own geometry.
    private RoadSegmentWasAdded BuildSegment(
        int id,
        RoadNodeWasAdded? startNode,
        RoadNodeWasAdded? endNode,
        RoadSegmentGeometry geometry,
        RoadSegmentStatusV2 status,
        RoadSegmentGeometryDrawMethodV2? geometryDrawMethod = null)
    {
        var template = TestData.Segment1Added;

        return template with
        {
            RoadSegmentId = new RoadSegmentId(id),
            StartNodeId = startNode?.RoadNodeId,
            EndNodeId = endNode?.RoadNodeId,
            Geometry = geometry,
            GeometryDrawMethod = geometryDrawMethod ?? RoadSegmentGeometryDrawMethodV2.Ingeschetst,
            Status = status,
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

    private ScopedRoadNetwork BuildNetwork(RoadNodeWasAdded[] nodes, RoadSegmentWasAdded[] segments)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            nodes.Select(x => RoadNode.Create(x).WithoutChanges()).ToArray(),
            segments.Select(x => RoadSegment.Create(x).WithoutChanges()).ToArray(),
            [],
            []).WithoutChanges();
    }

    // An existing realized road from (0,0) to (200,0), already split at (100,0): the middle node is the one a planned
    // segment can hook onto. The planned segment runs north from there, its end point given by the caller.
    private ScopedRoadNetwork BuildNetworkWith(RoadSegmentGeometry plannedGeometry, RoadSegmentStatusV2? plannedStatus = null, RoadSegmentGeometryDrawMethodV2? plannedDrawMethod = null)
    {
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(11, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var eastNode = BuildNode(12, 200, 0, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [westNode, middleNode, eastNode],
            [
                BuildSegment(PlannedSegmentId, null, null, plannedGeometry, plannedStatus ?? RoadSegmentStatusV2.Gepland, plannedDrawMethod),
                BuildSegment(RealizedSegmentId, westNode, middleNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(SecondRealizedSegmentId, middleNode, eastNode, BuildGeometry((100, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd)
            ]);
    }

    private static InMemoryRoadNetworkIdGenerator IdGenerator()
    {
        return new InMemoryRoadNetworkIdGenerator(initialValue: 100);
    }

    private RoadNetworkChangeResult Act(ScopedRoadNetwork roadNetwork, bool mayModifyMeasuredRoadSegments = true, int roadSegmentId = PlannedSegmentId)
    {
        return roadNetwork.RealizeRoadSegment(
            new RealizeRoadSegmentChange { RoadSegmentId = new RoadSegmentId(roadSegmentId) },
            mayModifyMeasuredRoadSegments,
            IdGenerator(),
            TestData.Provenance);
    }

    [Fact]
    public void WhenTheStartPointIsOnAnExistingNode_ThenItConnectsToItAndBecomesRealized()
    {
        // Drawn exactly on the existing node at (100,0), so nothing has to move.
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (100, 80)));

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();

        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(PlannedSegmentId)];
        roadSegment.Status.Should().Be(RoadSegmentStatusV2.Gerealiseerd);
        roadSegment.StartNodeId.Should().Be(new RoadNodeId(11));

        // The far end found nothing, so it got an end node of its own.
        result.Summary.RoadNodes.Added.Should().ContainSingle();
        roadSegment.EndNodeId.Should().Be(result.Summary.RoadNodes.Added.Single());
    }

    [Fact]
    public void WhenTheStartPointIsNearAnExistingNode_ThenTheGeometryIsSnappedOntoIt()
    {
        // 40cm off the node at (100,0), which is within the one metre reach.
        var roadNetwork = BuildNetworkWith(BuildGeometry((100.4, 0), (100.4, 80)));

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();

        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(PlannedSegmentId)];
        roadSegment.StartNodeId.Should().Be(new RoadNodeId(11));

        // Only the first vertex moved; the one after it stayed exactly where it was drawn.
        var coordinates = roadSegment.Geometry.Value.GetSingleLineString().Coordinates;
        coordinates[0].X.Should().BeApproximately(100, 0.001);
        coordinates[0].Y.Should().BeApproximately(0, 0.001);
        coordinates[^1].X.Should().BeApproximately(100.4, 0.001);
    }

    [Fact]
    public void WhenAnExistingNodeIsJustOutOfReach_ThenAnEndNodeIsAddedInstead()
    {
        // 1.4m away from the node at (100,0): too far to snap onto, so this end terminates on a node of its own. The
        // other end is drawn on the node so the segment still connects to the network.
        var roadNetwork = BuildNetworkWith(BuildGeometry((101.4, 80), (100, 0)));

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();

        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(PlannedSegmentId)];
        roadSegment.EndNodeId.Should().Be(new RoadNodeId(11));

        var addedNodeId = result.Summary.RoadNodes.Added.Should().ContainSingle().Which;
        roadSegment.StartNodeId.Should().Be(addedNodeId);

        // Placed where the segment was drawn, not pulled towards the node it could not reach.
        roadNetwork.RoadNodes[addedNodeId].Geometry.Value.Coordinate.X.Should().BeApproximately(101.4, 0.001);
    }

    [Fact]
    public void WhenTwoNodesAreInReach_ThenItSnapsOntoTheNearest()
    {
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(11, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var eastNode = BuildNode(12, 200, 0, RoadNodeTypeV2.Eindknoop);
        // A stub hanging south, so there is a second road node within reach of the planned start point.
        var stubNode = BuildNode(13, 100.9, 0, RoadNodeTypeV2.Eindknoop);
        var stubEndNode = BuildNode(14, 100.9, -60, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [westNode, middleNode, eastNode, stubNode, stubEndNode],
            [
                // 30cm to the middle node against 60cm to the stub node.
                BuildSegment(PlannedSegmentId, null, null, BuildGeometry((100.3, 0), (100.3, 80)), RoadSegmentStatusV2.Gepland),
                BuildSegment(RealizedSegmentId, westNode, middleNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(SecondRealizedSegmentId, middleNode, eastNode, BuildGeometry((100, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd),
                BuildSegment(4, stubNode, stubEndNode, BuildGeometry((100.9, 0), (100.9, -60)), RoadSegmentStatusV2.Gerealiseerd)
            ]);

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(PlannedSegmentId)].StartNodeId.Should().Be(new RoadNodeId(11));
    }

    [Fact]
    public void WhenNeitherEndPointHasANodeInReach_ThenError()
    {
        // VAL-5: an island. Far away from the existing road at y = 0.
        var roadNetwork = BuildNetworkWith(BuildGeometry((500, 500), (500, 580)));

        var result = Act(roadNetwork);

        result.Problems.Select(x => x.Reason).Should().Contain(RoadSegmentRealizeNoRoadNodeInReach.ProblemCode.ToString());
        roadNetwork.RoadSegments[new RoadSegmentId(PlannedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gepland);
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.Gerealiseerd))]
    [InlineData(nameof(RoadSegmentStatusV2.BuitenGebruik))]
    public void WhenTheStatusIsNotGepland_ThenError(string status)
    {
        // VAL-4
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (100, 80)), RoadSegmentStatusV2.Parse(status));

        var result = Act(roadNetwork);

        result.Problems.Select(x => x.Reason).Should().Contain(RoadSegmentRealizeStatusNotValid.ProblemCode.ToString());
    }

    [Fact]
    public void WhenTheSegmentIsMeasuredAndTheCallerMayNotTouchThose_ThenError()
    {
        // VAL-9
        var roadNetwork = BuildNetworkWith(
            BuildGeometry((100, 0), (100, 80)),
            plannedDrawMethod: RoadSegmentGeometryDrawMethodV2.Ingemeten);

        var result = Act(roadNetwork, mayModifyMeasuredRoadSegments: false);

        result.Problems.Select(x => x.Reason).Should().Contain(RoadSegmentRealizeMeasuredNotAllowed.ProblemCode.ToString());
        roadNetwork.RoadSegments[new RoadSegmentId(PlannedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gepland);
    }

    [Fact]
    public void WhenTheSegmentIsMeasuredAndTheCallerMayTouchThose_ThenItIsRealized()
    {
        var roadNetwork = BuildNetworkWith(
            BuildGeometry((100, 0), (100, 80)),
            plannedDrawMethod: RoadSegmentGeometryDrawMethodV2.Ingemeten);

        var result = Act(roadNetwork, mayModifyMeasuredRoadSegments: true);

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(PlannedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gerealiseerd);
    }

    [Fact]
    public void WhenTheSegmentDoesNotExist_ThenNotFound()
    {
        // VAL-2
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (100, 80)));

        var result = Act(roadNetwork, roadSegmentId: 999);

        result.Problems.Select(x => x.Reason).Should().Contain(ProblemCode.RoadSegment.NotFound.ToString());
    }

    [Fact]
    public void WhenTheSegmentCrossesARealizedSegment_ThenAGradeJunctionIsAdded()
    {
        // Hooks onto the node at (100,0) and runs back west across the realized road at y = 0.
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (50, 40), (20, -40)));

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();
        result.Summary.GradeJunctions.Added.Should().ContainSingle();
    }

    [Fact]
    public void WhenTheSegmentHasTheSameStartAndEndPoint_ThenError()
    {
        // VAL-7
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (140, 40), (60, 40), (100, 0)));

        var result = Act(roadNetwork);

        result.Problems.Should().NotBeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(PlannedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gepland);
    }

    [Fact]
    public void WhenTheSegmentCrossesItself_ThenError()
    {
        // VAL-8
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (100, 80), (60, 40), (140, 40)));

        var result = Act(roadNetwork);

        result.Problems.Should().NotBeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(PlannedSegmentId)].Status.Should().Be(RoadSegmentStatusV2.Gepland);
    }

    [Fact]
    public void WhenTheSegmentIsRealized_ThenTheNodeItConnectedToIsRetyped()
    {
        // The node at (100,0) had two segments and was a 'validatieknoop'; with a third it is an 'echte knoop'.
        var roadNetwork = BuildNetworkWith(BuildGeometry((100, 0), (100, 80)));

        var result = Act(roadNetwork);

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadNodes[new RoadNodeId(11)].Type.Should().Be(RoadNodeTypeV2.EchteKnoop);
    }
}
