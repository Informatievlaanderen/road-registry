namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ModifyRoadSegmentGeometry;

using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using GradeJunction = RoadRegistry.GradeJunction.GradeJunction;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    // Segment A runs west to east, segment B hangs off its end node and runs north. Changing A's end vertex therefore
    // drags the shared node - and with it B's start vertex - along.
    private const int SegmentAId = 1;
    private const int SegmentBId = 2;
    private const int SegmentCId = 3;

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

    // All attribute values cover the whole segment, so a segment built this way is always internally consistent with
    // its own geometry - which is what the attribute validation demands on every later change.
    private RoadSegmentWasAdded BuildSegment(
        int id,
        RoadNodeWasAdded startNode,
        RoadNodeWasAdded endNode,
        RoadSegmentGeometry geometry,
        RoadSegmentGeometryDrawMethodV2? geometryDrawMethod = null,
        RoadSegmentStatusV2? status = null)
    {
        var template = TestData.Segment1Added;

        return template with
        {
            RoadSegmentId = new RoadSegmentId(id),
            StartNodeId = startNode.RoadNodeId,
            EndNodeId = endNode.RoadNodeId,
            Geometry = geometry,
            GeometryDrawMethod = geometryDrawMethod ?? RoadSegmentGeometryDrawMethodV2.Ingeschetst,
            Status = status ?? RoadSegmentStatusV2.Gerealiseerd,
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

    // The attributes a caller has to send along with a new geometry: every mandatory attribute, over the full length
    // of the geometry being submitted.
    private ModifyRoadSegmentGeometryChange BuildChange(ScopedRoadNetwork roadNetwork, int roadSegmentId, RoadSegmentGeometry geometry)
    {
        var attributes = roadNetwork.RoadSegments[new RoadSegmentId(roadSegmentId)].Attributes!;

        return new ModifyRoadSegmentGeometryChange
        {
            RoadSegmentId = new RoadSegmentId(roadSegmentId),
            Geometry = geometry,
            AccessRestriction = Spanning(attributes.AccessRestriction, geometry),
            Category = Spanning(attributes.Category, geometry),
            Morphology = Spanning(attributes.Morphology, geometry),
            StreetNameId = Spanning(attributes.StreetNameId, geometry),
            MaintenanceAuthorityId = Spanning(attributes.MaintenanceAuthorityId, geometry),
            SurfaceType = Spanning(attributes.SurfaceType, geometry),
            CarTrafficDirection = Spanning(attributes.CarTrafficDirection, geometry),
            BikeTrafficDirection = Spanning(attributes.BikeTrafficDirection, geometry),
            PedestrianTrafficDirection = Spanning(attributes.PedestrianTrafficDirection, geometry)
        };
    }

    private ScopedRoadNetwork BuildNetwork(
        RoadNodeWasAdded[] nodes,
        RoadSegmentWasAdded[] segments,
        GradeSeparatedJunctionWasAdded[]? gradeSeparatedJunctions = null,
        GradeJunctionWasAdded[]? gradeJunctions = null)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            nodes.Select(x => RoadNode.Create(x).WithoutChanges()).ToArray(),
            segments.Select(x => RoadSegment.Create(x).WithoutChanges()).ToArray(),
            (gradeSeparatedJunctions ?? []).Select(x => GradeSeparatedJunction.Create(x).WithoutChanges()).ToArray(),
            (gradeJunctions ?? []).Select(x => GradeJunction.Create(x).WithoutChanges()).ToArray()).WithoutChanges();
    }

    private static RoadSegmentWasAdded WithTraffic(
        RoadSegmentWasAdded segment,
        RoadSegmentTrafficDirection car,
        RoadSegmentTrafficDirection bike,
        RoadSegmentPedestrianTrafficDirection pedestrian)
    {
        return segment with
        {
            CarTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>().Add(car, segment.Geometry),
            BikeTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>().Add(bike, segment.Geometry),
            PedestrianTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>().Add(pedestrian, segment.Geometry)
        };
    }

    private JunctionGeometry BuildJunctionGeometry(double x, double y)
    {
        return JunctionGeometry.Create(new Point(new Coordinate(x, y)) { SRID = WellknownSrids.Lambert08 });
    }

    // A single segment (0,0) -> (100,0) with an end node at each side.
    private ScopedRoadNetwork BuildNetworkWithSingleSegment(
        RoadSegmentGeometryDrawMethodV2? geometryDrawMethod = null,
        RoadSegmentStatusV2? status = null)
    {
        var startNode = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var endNode = BuildNode(2, 100, 0, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [startNode, endNode],
            [BuildSegment(SegmentAId, startNode, endNode, BuildGeometry((0, 0), (100, 0)), geometryDrawMethod, status)]);
    }

    private static InMemoryRoadNetworkIdGenerator IdGenerator()
    {
        return new InMemoryRoadNetworkIdGenerator(initialValue: 100);
    }

    [Fact]
    public void WhenOnlyAnInnerVertexMoves_ThenOnlyTheSegmentChanges()
    {
        var roadNetwork = BuildNetworkWithSingleSegment();
        var geometry = BuildGeometry((0, 0), (50, 10), (100, 0));

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, geometry), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.RoadSegments.Modified.Should().ContainSingle().Which.Should().Be(new RoadSegmentId(SegmentAId));

        // The endpoints did not move, so no road node was touched.
        result.Summary.RoadNodes.Modified.Should().BeEmpty();
        roadNetwork.RoadNodes.Values.Should().OnlyContain(x => !x.GetChanges().Any());
    }

    [Fact]
    public void WhenAnEndVertexMoves_ThenTheRoadNodeMovesAlongAndKeepsItsIdentifier()
    {
        var roadNetwork = BuildNetworkWithSingleSegment();
        var geometry = BuildGeometry((0, 0), (110, 0));

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, geometry), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();

        var endNodeId = new RoadNodeId(2);
        result.Summary.RoadNodes.Modified.Should().ContainSingle().Which.Should().Be(endNodeId);
        result.Summary.RoadNodes.Added.Should().BeEmpty();
        result.Summary.RoadNodes.Removed.Should().BeEmpty();

        // Identifier preserved: the node is modified, never replaced.
        var endNode = roadNetwork.RoadNodes[endNodeId];
        endNode.GetChanges().OfType<RoadNodeWasModified>().Should().ContainSingle()
            .Which.RoadNodeId.Should().Be(endNodeId);
        endNode.Geometry.Value.Coordinate.X.Should().Be(110);
        endNode.Geometry.Value.Coordinate.Y.Should().Be(0);
    }

    [Fact]
    public void WhenARoadNodeMoves_ThenConnectedSegmentsFollowAndTheirAttributesAreRescaled()
    {
        // Segment B is dynamically segmented over two stretches; when it gets longer the division between them has to
        // move with it, keeping the same proportion (the rule LARA applies).
        var startNodeA = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var sharedNode = BuildNode(2, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var endNodeB = BuildNode(3, 100, 100, RoadNodeTypeV2.Eindknoop);

        var geometryB = BuildGeometry((100, 0), (100, 100));
        var segmentB = BuildSegment(SegmentBId, sharedNode, endNodeB, geometryB);

        var morphologies = RoadSegmentMorphologyV2.All.Take(2).ToArray();
        segmentB = segmentB with
        {
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>()
                .Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(20), morphologies[0])
                .Add(new RoadSegmentPositionV2(20), new RoadSegmentPositionV2(100), morphologies[1])
        };

        var roadNetwork = BuildNetwork(
            [startNodeA, sharedNode, endNodeB],
            [BuildSegment(SegmentAId, startNodeA, sharedNode, BuildGeometry((0, 0), (100, 0))), segmentB]);

        // Stretch A eastward by 10m: the shared node moves to (110,0) and B becomes sqrt(10^2+100^2) = 100.50m long.
        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (110, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.RoadSegments.Modified.Should().BeEquivalentTo([new RoadSegmentId(SegmentAId), new RoadSegmentId(SegmentBId)]);

        var modifiedB = roadNetwork.RoadSegments[new RoadSegmentId(SegmentBId)];
        var lineB = modifiedB.Geometry.Value.GetSingleLineString();
        lineB.Coordinates[0].X.Should().Be(110);
        lineB.Coordinates[0].Y.Should().Be(0);
        lineB.Coordinates[^1].X.Should().Be(100);
        lineB.Coordinates[^1].Y.Should().Be(100);

        var newLength = modifiedB.Geometry.Value.Length.RoundToCm();
        newLength.Should().Be(100.5);

        // 20 x (100.50 / 100) = 20.10, and the trailing position lands exactly on the new length.
        var coverages = modifiedB.Attributes!.Morphology.Values.OrderBy(x => x.Coverage.From).ToArray();
        coverages.Should().HaveCount(2);
        coverages[0].Coverage.From.ToDouble().Should().Be(0);
        coverages[0].Coverage.To.ToDouble().Should().Be(20.1);
        coverages[1].Coverage.From.ToDouble().Should().Be(20.1);
        coverages[1].Coverage.To.ToDouble().Should().Be(newLength);

        modifiedB.GetChanges().OfType<RoadSegmentWasModified>().Should().ContainSingle();
    }

    [Fact]
    public void WhenTheMovedRoadNodeIsTheConnectedSegmentsEndNode_ThenItIsRescaledFromThatSide()
    {
        // The mirror of the test above: the moved node is now segment B's END rather than its start. B is a straight
        // two-vertex segment, so it consists of nothing but the stretch that changed and the whole segmentation
        // follows it either way - see the three-vertex test below for the case where that distinction bites.
        var startNodeA = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var sharedNode = BuildNode(2, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var startNodeB = BuildNode(3, 100, 100, RoadNodeTypeV2.Eindknoop);

        // B runs towards the shared node, so position 0 sits at the far end that stays put.
        var segmentB = BuildSegment(SegmentBId, startNodeB, sharedNode, BuildGeometry((100, 100), (100, 0)));

        var morphologies = RoadSegmentMorphologyV2.All.Take(2).ToArray();
        segmentB = segmentB with
        {
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>()
                .Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(20), morphologies[0])
                .Add(new RoadSegmentPositionV2(20), new RoadSegmentPositionV2(100), morphologies[1])
        };

        var roadNetwork = BuildNetwork(
            [startNodeA, sharedNode, startNodeB],
            [BuildSegment(SegmentAId, startNodeA, sharedNode, BuildGeometry((0, 0), (100, 0))), segmentB]);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (110, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();

        var modifiedB = roadNetwork.RoadSegments[new RoadSegmentId(SegmentBId)];
        var lineB = modifiedB.Geometry.Value.GetSingleLineString();

        // The start vertex is the one that stayed; the end vertex followed the node.
        lineB.Coordinates[0].X.Should().Be(100);
        lineB.Coordinates[0].Y.Should().Be(100);
        lineB.Coordinates[^1].X.Should().Be(110);
        lineB.Coordinates[^1].Y.Should().Be(0);

        var newLength = modifiedB.Geometry.Value.Length.RoundToCm();
        newLength.Should().Be(100.5);

        var coverages = modifiedB.Attributes!.Morphology.Values.OrderBy(x => x.Coverage.From).ToArray();
        coverages.Should().HaveCount(2);
        coverages[0].Coverage.From.ToDouble().Should().Be(0);
        coverages[0].Coverage.To.ToDouble().Should().Be(20.1);
        coverages[1].Coverage.From.ToDouble().Should().Be(20.1);
        coverages[1].Coverage.To.ToDouble().Should().Be(newLength);
    }

    [Fact]
    public void WhenTheRoadNodeWouldBeDraggedTooFar_ThenError()
    {
        // A 'validatieknoop' may travel at most 20m.
        var startNode = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var endNode = BuildNode(2, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var roadNetwork = BuildNetwork(
            [startNode, endNode],
            [BuildSegment(SegmentAId, startNode, endNode, BuildGeometry((0, 0), (100, 0)))]);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (130, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentChangeGeometryRoadNodeMovedTooFar");
        roadNetwork.RoadNodes[new RoadNodeId(2)].GetChanges().Should().BeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(SegmentAId)].GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenAnEndKnotIsDraggedFurtherThanOtherNodesMay_ThenItIsStillAccepted()
    {
        // Same distance as the previous test, but an 'eindknoop' is allowed up to 100m.
        var roadNetwork = BuildNetworkWithSingleSegment();

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (130, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.RoadNodes.Modified.Should().ContainSingle().Which.Should().Be(new RoadNodeId(2));
    }

    [Fact]
    public void WhenTheNewEndVertexLandsTooCloseToAnotherRoadNode_ThenError()
    {
        // Moving onto another node would change the topology, which this action never does.
        var startNodeA = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var endNodeA = BuildNode(2, 100, 0, RoadNodeTypeV2.Eindknoop);
        var startNodeB = BuildNode(3, 110, 0, RoadNodeTypeV2.Eindknoop);
        var endNodeB = BuildNode(4, 110, 100, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [startNodeA, endNodeA, startNodeB, endNodeB],
            [
                BuildSegment(SegmentAId, startNodeA, endNodeA, BuildGeometry((0, 0), (100, 0))),
                BuildSegment(SegmentBId, startNodeB, endNodeB, BuildGeometry((110, 0), (110, 100)))
            ]);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (109.5, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentChangeGeometryPointTooCloseToRoadNode");
    }

    [Fact]
    public void WhenBothEndVerticesMove_ThenTheOtherNodesVacatedPositionIsNotHeldAgainstTheMove()
    {
        // A short segment whose start is pulled towards where its end node currently sits, while that end node is
        // itself pulled away. Measuring the new start against the end node's old position would reject this, but by
        // the time the change lands the two are 20m apart.
        var startNode = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var endNode = BuildNode(2, 10, 0, RoadNodeTypeV2.Eindknoop);
        var roadNetwork = BuildNetwork(
            [startNode, endNode],
            [BuildSegment(SegmentAId, startNode, endNode, BuildGeometry((0, 0), (10, 0)))]);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((9.5, 0), (30, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.RoadNodes.Modified.Should().BeEquivalentTo([new RoadNodeId(1), new RoadNodeId(2)]);
        roadNetwork.RoadNodes[new RoadNodeId(1)].Geometry.Value.Coordinate.X.Should().Be(9.5);
        roadNetwork.RoadNodes[new RoadNodeId(2)].Geometry.Value.Coordinate.X.Should().Be(30);
    }

    [Fact]
    public void WhenBothMovedRoadNodesWouldEndUpTooCloseTogether_ThenError()
    {
        // The mirror image: both nodes move, and their destinations are less than a metre apart. Neither is in the
        // other's way at the position it is leaving, so only comparing the two destinations catches this.
        var roadNetwork = BuildNetworkWithSingleSegment();

        // A hairpin: 4.5m of geometry, but the two ends finish 0.5m apart.
        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((10, 0), (10, 2), (10.5, 2), (10.5, 0))),
            true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentChangeGeometryPointTooCloseToRoadNode");
        roadNetwork.RoadNodes.Values.Should().OnlyContain(x => !x.GetChanges().Any());
        roadNetwork.RoadSegments[new RoadSegmentId(SegmentAId)].GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenAConnectedSegmentHasAnIntermediateVertex_ThenOnlyTheStretchUpToItIsRescaled()
    {
        // Segment B bends at a vertex 40m along: (100,0) -> (100,40) -> (100,100). Only the stretch between the moved
        // node and that vertex changes length, so only the coverages inside it are rescaled; the rest keeps the length
        // it covers and shifts along. Stretching B as a whole would move the boundary at 40 away from the vertex it
        // was placed against, on a piece of road that did not move at all.
        var startNodeA = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var sharedNode = BuildNode(2, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var endNodeB = BuildNode(3, 100, 100, RoadNodeTypeV2.Eindknoop);

        var segmentB = BuildSegment(SegmentBId, sharedNode, endNodeB, BuildGeometry((100, 0), (100, 40), (100, 100)));

        var morphologies = RoadSegmentMorphologyV2.All.Take(4).ToArray();
        segmentB = segmentB with
        {
            Morphology = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>()
                .Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(20), morphologies[0])
                .Add(new RoadSegmentPositionV2(20), new RoadSegmentPositionV2(40), morphologies[1])
                .Add(new RoadSegmentPositionV2(40), new RoadSegmentPositionV2(70), morphologies[2])
                .Add(new RoadSegmentPositionV2(70), new RoadSegmentPositionV2(100), morphologies[3])
        };

        var roadNetwork = BuildNetwork(
            [startNodeA, sharedNode, endNodeB],
            [BuildSegment(SegmentAId, startNodeA, sharedNode, BuildGeometry((0, 0), (100, 0))), segmentB]);

        // The shared node drops 10m, so B's first stretch grows from 40m to 50m and B becomes 110m long.
        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (100, -10))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();

        var modifiedB = roadNetwork.RoadSegments[new RoadSegmentId(SegmentBId)];
        modifiedB.Geometry.Value.Length.RoundToCm().Should().Be(110);

        var coverages = modifiedB.Attributes!.Morphology.Values.OrderBy(x => x.Coverage.From).ToArray();
        coverages.Should().HaveCount(4);

        // Inside the stretch that grew: 20 x (50/40) = 25, and the vertex boundary lands on 50.
        coverages[0].Coverage.To.ToDouble().Should().Be(25);
        coverages[1].Coverage.To.ToDouble().Should().Be(50);

        // Beyond the vertex nothing was stretched: 30m each, exactly as before, shifted by the 10m gained.
        coverages[2].Coverage.From.ToDouble().Should().Be(50);
        coverages[2].Coverage.To.ToDouble().Should().Be(80);
        coverages[3].Coverage.From.ToDouble().Should().Be(80);
        coverages[3].Coverage.To.ToDouble().Should().Be(110);
    }

    // Segment A plus an unrelated segment B hanging off its own node 10m further east, so a move of A's end vertex can
    // be aimed at node 3.
    private ScopedRoadNetwork BuildNetworkWithASeparateSegmentNearby()
    {
        var startNodeA = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var endNodeA = BuildNode(2, 100, 0, RoadNodeTypeV2.Eindknoop);
        var startNodeB = BuildNode(3, 110, 0, RoadNodeTypeV2.Eindknoop);
        var endNodeB = BuildNode(4, 110, 100, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [startNodeA, endNodeA, startNodeB, endNodeB],
            [
                BuildSegment(SegmentAId, startNodeA, endNodeA, BuildGeometry((0, 0), (100, 0))),
                BuildSegment(SegmentBId, startNodeB, endNodeB, BuildGeometry((110, 0), (110, 100)))
            ]);
    }

    [Fact]
    public void WhenTheNewEndVertexLandsExactlyOnAnotherRoadNode_ThenError()
    {
        // The sharpest form of VAL-21: not merely close to another node but exactly on it, which would silently
        // reconnect the segment to a different node.
        var roadNetwork = BuildNetworkWithASeparateSegmentNearby();

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (110, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentChangeGeometryPointTooCloseToRoadNode");
        roadNetwork.RoadNodes[new RoadNodeId(2)].GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenTheNewEndVertexIsExactlyTheMinimumDistanceFromAnotherRoadNode_ThenItIsAccepted()
    {
        // The boundary of the same rule: a metre of clearance is enough. Rejecting this would block legitimate edits
        // without any test noticing.
        var roadNetwork = BuildNetworkWithASeparateSegmentNearby();

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (109, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadNodes[new RoadNodeId(2)].Geometry.Value.Coordinate.X.Should().Be(109);
    }

    [Fact]
    public void WhenTheRoadNodeIsDraggedExactlyTheMaximumDistance_ThenItIsAccepted()
    {
        // The boundary of VAL-22: a 'validatieknoop' may travel 20m, so exactly 20m is still allowed.
        var startNode = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var endNode = BuildNode(2, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var roadNetwork = BuildNetwork(
            [startNode, endNode],
            [BuildSegment(SegmentAId, startNode, endNode, BuildGeometry((0, 0), (100, 0)))]);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (120, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadNodes[new RoadNodeId(2)].Geometry.Value.Coordinate.X.Should().Be(120);
    }

    [Fact]
    public void WhenTheSegmentSitsOnNoRoadNodeAtAll_ThenNothingIsDragged()
    {
        // A 'gepland' segment is not knotted into the network yet, so its geometry moves on its own.
        var geometry = BuildGeometry((0, 0), (100, 0));
        var segment = BuildSegment(SegmentAId, BuildNode(1, 0, 0), BuildNode(2, 100, 0), geometry, status: RoadSegmentStatusV2.Gepland)
            with { StartNodeId = null, EndNodeId = null };

        var roadNetwork = BuildNetwork([], [segment]);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (110, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.RoadNodes.Modified.Should().BeEmpty();
        result.Summary.RoadSegments.Modified.Should().ContainSingle().Which.Should().Be(new RoadSegmentId(SegmentAId));
    }

    // Segment A runs west to east; segment C sits south of it and is only reached once A's middle vertex is pushed
    // down to (50,-25). A's end vertices - and therefore its road nodes - never move, so nothing else is in play.
    private ScopedRoadNetwork BuildNetworkWithASegmentToCross(
        RoadSegmentTrafficDirection carOnC,
        RoadSegmentTrafficDirection bikeOnC,
        RoadSegmentPedestrianTrafficDirection pedestrianOnC,
        RoadSegmentGeometry? geometryA = null,
        GradeSeparatedJunctionWasAdded[]? gradeSeparatedJunctions = null,
        GradeJunctionWasAdded[]? gradeJunctions = null)
    {
        var startNodeA = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var endNodeA = BuildNode(2, 100, 0, RoadNodeTypeV2.Eindknoop);
        var startNodeC = BuildNode(3, 50, -40, RoadNodeTypeV2.Eindknoop);
        var endNodeC = BuildNode(4, 50, -10, RoadNodeTypeV2.Eindknoop);

        // Only cars on A, only bikes or nothing on C, unless a test says otherwise.
        var segmentA = WithTraffic(
            BuildSegment(SegmentAId, startNodeA, endNodeA, geometryA ?? BuildGeometry((0, 0), (100, 0))),
            RoadSegmentTrafficDirection.Forward, RoadSegmentTrafficDirection.None, RoadSegmentPedestrianTrafficDirection.None);
        var segmentC = WithTraffic(
            BuildSegment(SegmentCId, startNodeC, endNodeC, BuildGeometry((50, -40), (50, -10))),
            carOnC, bikeOnC, pedestrianOnC);

        return BuildNetwork([startNodeA, endNodeA, startNodeC, endNodeC], [segmentA, segmentC], gradeSeparatedJunctions, gradeJunctions);
    }

    private RoadSegmentGeometry GeometryCrossingSegmentC()
    {
        return BuildGeometry((0, 0), (50, -25), (100, 0));
    }

    [Fact]
    public void WhenTheNewGeometryCrossesAnotherSegment_ThenAGradeJunctionIsAdded()
    {
        // A crossing that appears is simply recorded, whatever traffic the two segments carry: both admit cars here,
        // which the road network change flow would have refused without an ongelijkgrondse kruising.
        var roadNetwork = BuildNetworkWithASegmentToCross(
            RoadSegmentTrafficDirection.Forward, RoadSegmentTrafficDirection.None, RoadSegmentPedestrianTrafficDirection.None);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, GeometryCrossingSegmentC()), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.GradeJunctions.Added.Should().ContainSingle();

        var gradeJunction = roadNetwork.GradeJunctions.Values.Single(x => !x.IsRemoved);
        new[] { gradeJunction.RoadSegmentId1, gradeJunction.RoadSegmentId2 }
            .Should().BeEquivalentTo([new RoadSegmentId(SegmentAId), new RoadSegmentId(SegmentCId)]);
        gradeJunction.Geometry!.Value.Coordinate.X.Should().Be(50);
        gradeJunction.Geometry.Value.Coordinate.Y.Should().Be(-25);
    }

    [Fact]
    public void WhenTheCrossingSegmentsShareNoTrafficType_ThenAGradeJunctionIsAddedJustTheSame()
    {
        // Traffic is not consulted at all here, so the outcome is the one above whichever modes the segments carry.
        var roadNetwork = BuildNetworkWithASegmentToCross(
            RoadSegmentTrafficDirection.None, RoadSegmentTrafficDirection.Both, RoadSegmentPedestrianTrafficDirection.None);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, GeometryCrossingSegmentC()), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.GradeJunctions.Added.Should().ContainSingle();
    }

    [Fact]
    public void WhenTheCrossingIsAlreadyAGradeSeparatedJunction_ThenNoGradeJunctionIsAdded()
    {
        // The crossing is already recorded as an ongelijkgrondse kruising, so it is not a new intersection and keeps
        // what records it.
        var gradeSeparatedJunction = new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(SegmentCId),
            UpperRoadSegmentId = new RoadSegmentId(SegmentAId),
            Type = GradeSeparatedJunctionTypeV2.Brug,
            Geometry = BuildJunctionGeometry(50, -25),
            Provenance = new ProvenanceData(TestData.Provenance)
        };

        var roadNetwork = BuildNetworkWithASegmentToCross(
            RoadSegmentTrafficDirection.Forward, RoadSegmentTrafficDirection.None, RoadSegmentPedestrianTrafficDirection.None,
            gradeSeparatedJunctions: [gradeSeparatedJunction]);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, GeometryCrossingSegmentC()), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.GradeJunctions.Added.Should().BeEmpty();
        roadNetwork.GradeSeparatedJunctions[new GradeSeparatedJunctionId(1)].IsRemoved.Should().BeFalse();
    }

    [Fact]
    public void WhenTheNewGeometryNoLongerCrossesASegment_ThenItsGradeJunctionIsRemoved()
    {
        var gradeJunction = new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(SegmentAId),
            RoadSegmentId2 = new RoadSegmentId(SegmentCId),
            Geometry = BuildJunctionGeometry(50, -25),
            Provenance = new ProvenanceData(TestData.Provenance)
        };

        var roadNetwork = BuildNetworkWithASegmentToCross(
            RoadSegmentTrafficDirection.None, RoadSegmentTrafficDirection.Both, RoadSegmentPedestrianTrafficDirection.None,
            geometryA: GeometryCrossingSegmentC(),
            gradeJunctions: [gradeJunction]);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (100, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.GradeJunctions.Removed.Should().ContainSingle()
            .Which.Should().Be(new GradeJunctionId(1));
        roadNetwork.GradeJunctions[new GradeJunctionId(1)].IsRemoved.Should().BeTrue();
    }

    [Fact]
    public void WhenTheNewGeometryNoLongerCrossesASegment_ThenItsGradeSeparatedJunctionIsRemovedToo()
    {
        // A vanished crossing takes whatever recorded it, so an ongelijkgrondse kruising goes the same way.
        var gradeSeparatedJunction = new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(SegmentCId),
            UpperRoadSegmentId = new RoadSegmentId(SegmentAId),
            Type = GradeSeparatedJunctionTypeV2.Brug,
            Geometry = BuildJunctionGeometry(50, -25),
            Provenance = new ProvenanceData(TestData.Provenance)
        };

        var roadNetwork = BuildNetworkWithASegmentToCross(
            RoadSegmentTrafficDirection.Forward, RoadSegmentTrafficDirection.None, RoadSegmentPedestrianTrafficDirection.None,
            geometryA: GeometryCrossingSegmentC(),
            gradeSeparatedJunctions: [gradeSeparatedJunction]);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (100, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.GradeSeparatedJunctions.Removed.Should().ContainSingle()
            .Which.Should().Be(new GradeSeparatedJunctionId(1));
        roadNetwork.GradeSeparatedJunctions[new GradeSeparatedJunctionId(1)].IsRemoved.Should().BeTrue();
    }

    [Fact]
    public void WhenTheNewGeometryRunsAlongAnotherSegment_ThenError()
    {
        // The road segment topology verification: sharing a stretch of road with another segment rather than merely
        // crossing it.
        var roadNetwork = BuildNetworkWithASegmentToCross(
            RoadSegmentTrafficDirection.None, RoadSegmentTrafficDirection.Both, RoadSegmentPedestrianTrafficDirection.None);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (50, -40), (50, -10), (100, 0))),
            true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentPartiallyOverlapsWithAnotherRoadSegment");
        result.Summary.GradeJunctions.Added.Should().BeEmpty();
    }

    [Fact]
    public void WhenTheNewGeometrySelfIntersects_ThenError()
    {
        // VAL-20. The path doubles back over itself between its two end vertices.
        var roadNetwork = BuildNetworkWithSingleSegment();

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (100, 40), (0, 40), (100, 0))),
            true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentGeometrySelfIntersects");
        roadNetwork.RoadSegments[new RoadSegmentId(SegmentAId)].GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenTheNewGeometryHasTheSameStartAndEndPoint_ThenError()
    {
        // VAL-19. Caught before any road node is touched, so the end node is not dragged onto the start node either.
        var roadNetwork = BuildNetworkWithSingleSegment();

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (50, 50), (100, 0), (0, 0))),
            true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentGeometryStartEqualsEnd");
        roadNetwork.RoadNodes.Values.Should().OnlyContain(x => !x.GetChanges().Any());
        roadNetwork.RoadSegments[new RoadSegmentId(SegmentAId)].GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenTheNewGeometryCrossesTheSameSegmentTwice_ThenError()
    {
        // VAL-18. Segment D lies horizontally below A, and A is bent down under it and back up, cutting it twice.
        var startNodeA = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var endNodeA = BuildNode(2, 100, 0, RoadNodeTypeV2.Eindknoop);
        var startNodeD = BuildNode(3, 10, -20, RoadNodeTypeV2.Eindknoop);
        var endNodeD = BuildNode(4, 90, -20, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [startNodeA, endNodeA, startNodeD, endNodeD],
            [
                BuildSegment(SegmentAId, startNodeA, endNodeA, BuildGeometry((0, 0), (100, 0))),
                BuildSegment(SegmentCId, startNodeD, endNodeD, BuildGeometry((10, -20), (90, -20)))
            ]);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (40, -40), (60, -40), (100, 0))),
            true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentDuplicateIntersections");
        result.Summary.GradeJunctions.Added.Should().BeEmpty();
    }

    [Fact]
    public void WhenADraggedSegmentWouldSelfIntersect_ThenError()
    {
        // The realized rules apply to the segments dragged along too, not only to the one in the request: B is a hook
        // whose free end folds back over its own tail once its start vertex is pulled westwards.
        var startNodeA = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var sharedNode = BuildNode(2, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var endNodeB = BuildNode(3, 90, 2, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [startNodeA, sharedNode, endNodeB],
            [
                BuildSegment(SegmentAId, startNodeA, sharedNode, BuildGeometry((0, 0), (100, 0))),
                BuildSegment(SegmentBId, sharedNode, endNodeB, BuildGeometry((100, 0), (110, 20), (90, 20), (90, 2)))
            ]);

        // Pulling the shared node back to (85,0) swings B's first edge across its own last edge.
        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (85, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentGeometrySelfIntersects");
        result.Summary.RoadSegments.Modified.Should().NotContain(new RoadSegmentId(SegmentBId));
    }

    [Fact]
    public void WhenADraggedSegmentWouldCrossTheSameSegmentTwice_ThenError()
    {
        // VAL-18 on a segment dragged along rather than the one in the request. B already crosses D once over its top
        // edge; pulling its start vertex westwards swings its first edge across D as well.
        var startNodeA = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var sharedNode = BuildNode(2, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var endNodeB = BuildNode(3, 60, 20, RoadNodeTypeV2.Eindknoop);
        var startNodeD = BuildNode(4, 90, 10, RoadNodeTypeV2.Eindknoop);
        var endNodeD = BuildNode(5, 90, 70, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [startNodeA, sharedNode, endNodeB, startNodeD, endNodeD],
            [
                BuildSegment(SegmentAId, startNodeA, sharedNode, BuildGeometry((0, 0), (100, 0))),
                BuildSegment(SegmentBId, sharedNode, endNodeB, BuildGeometry((100, 0), (100, 50), (60, 50), (60, 20))),
                BuildSegment(SegmentCId, startNodeD, endNodeD, BuildGeometry((90, 10), (90, 70)))
            ]);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (85, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentDuplicateIntersections");
        result.Summary.GradeJunctions.Added.Should().BeEmpty();
    }

    [Fact]
    public void WhenADragWouldCloseAConnectedSegmentIntoALoop_ThenTheNodeProximityRuleBlocksItFirst()
    {
        // VAL-19 cannot be reached on a dragged segment. Its start and end vertices sit on its two road nodes, so
        // they can only coincide if the moved node lands on the other one - and VAL-21 forbids coming within a metre
        // of a road node that is not itself moving. B hangs off the shared node and returns to its own end node 10m
        // away; dragging the shared node onto it is refused before the loop can be formed.
        var startNodeA = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var sharedNode = BuildNode(2, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var endNodeB = BuildNode(3, 90, 0, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [startNodeA, sharedNode, endNodeB],
            [
                BuildSegment(SegmentAId, startNodeA, sharedNode, BuildGeometry((0, 0), (100, 0))),
                BuildSegment(SegmentBId, sharedNode, endNodeB, BuildGeometry((100, 0), (95, 30), (90, 0)))
            ]);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (90, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentChangeGeometryPointTooCloseToRoadNode");
        roadNetwork.RoadSegments[new RoadSegmentId(SegmentBId)].GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenTheSegmentIsMeasuredAndTheCallerMayNotEditMeasuredSegments_ThenError()
    {
        var roadNetwork = BuildNetworkWithSingleSegment(RoadSegmentGeometryDrawMethodV2.Ingemeten);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (110, 0))), false, IdGenerator(), TestData.Provenance);

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentChangeGeometryMeasuredNotAllowed");
        roadNetwork.RoadSegments[new RoadSegmentId(SegmentAId)].GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenAConnectedSegmentIsMeasuredAndTheCallerMayNotEditMeasuredSegments_ThenError()
    {
        // VAL-5 catches the whole operation, not just the segment named in the request: a decentral manager may not
        // drag a measured segment along either.
        var startNodeA = BuildNode(1, 0, 0, RoadNodeTypeV2.Eindknoop);
        var sharedNode = BuildNode(2, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var endNodeB = BuildNode(3, 100, 100, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [startNodeA, sharedNode, endNodeB],
            [
                BuildSegment(SegmentAId, startNodeA, sharedNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentGeometryDrawMethodV2.Ingeschetst),
                BuildSegment(SegmentBId, sharedNode, endNodeB, BuildGeometry((100, 0), (100, 100)), RoadSegmentGeometryDrawMethodV2.Ingemeten)
            ]);

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (110, 0))), false, IdGenerator(), TestData.Provenance);

        var problem = result.Problems.Should().ContainSingle().Subject;
        problem.Reason.Should().Be("RoadSegmentChangeGeometryMeasuredNotAllowed");

        // The problem names the segment that is actually measured - the connected one - not the segment the request
        // was about. Reporting it under the requested segment's identifier would point at the wrong road.
        problem.Parameters.Should().ContainSingle(x => x.Name == "WegsegmentId")
            .Which.Value.Should().Be(SegmentBId.ToString());

        roadNetwork.RoadSegments[new RoadSegmentId(SegmentBId)].GetChanges().Should().BeEmpty();
        roadNetwork.RoadNodes[new RoadNodeId(2)].GetChanges().Should().BeEmpty();
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.NietGerealiseerd))]
    [InlineData(nameof(RoadSegmentStatusV2.Gehistoreerd))]
    public void WhenStatusIsNotEditable_ThenError(string statusName)
    {
        // VAL-4
        var roadNetwork = BuildNetworkWithSingleSegment(status: RoadSegmentStatusV2.Parse(statusName));

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (110, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentChangeGeometryStatusNotValid");
        roadNetwork.RoadSegments[new RoadSegmentId(SegmentAId)].GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenTheSegmentDoesNotExist_ThenError()
    {
        var roadNetwork = BuildNetworkWithSingleSegment();

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            new ModifyRoadSegmentGeometryChange
            {
                RoadSegmentId = new RoadSegmentId(999),
                Geometry = BuildGeometry((0, 0), (110, 0))
            }, true, IdGenerator(), TestData.Provenance);

        var problem = result.Problems.Should().ContainSingle().Subject;
        problem.Reason.Should().Be("RoadSegmentNotFound");

        // The error itself carries no identifier: it comes from the context the problems are collected under.
        problem.Parameters.Should().ContainSingle(x => x.Name == "WegsegmentId")
            .Which.Value.Should().Be("999");
    }

    [Fact]
    public void WhenTheGeometryIsShorterThanTheMinimumLength_ThenError()
    {
        // VAL-16
        var roadNetwork = BuildNetworkWithSingleSegment();

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, BuildGeometry((0, 0), (0.5, 0))), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().NotBeEmpty();
        roadNetwork.RoadSegments[new RoadSegmentId(SegmentAId)].GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenNothingActuallyMoves_ThenNoChangeSummaryIsEmitted()
    {
        // Resubmitting the geometry the segment already has records nothing, so no road network change event is raised.
        var roadNetwork = BuildNetworkWithSingleSegment();
        var geometry = roadNetwork.RoadSegments[new RoadSegmentId(SegmentAId)].Geometry;

        var result = roadNetwork.ModifyRoadSegmentGeometry(
            BuildChange(roadNetwork, SegmentAId, geometry), true, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.HasChanges().Should().BeFalse();
        roadNetwork.GetChanges().Should().BeEmpty();
    }
}
