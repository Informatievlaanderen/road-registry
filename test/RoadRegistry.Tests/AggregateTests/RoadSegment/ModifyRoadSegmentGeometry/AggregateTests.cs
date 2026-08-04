namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ModifyRoadSegmentGeometry;

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
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    // Segment A runs west to east, segment B hangs off its end node and runs north. Changing A's end vertex therefore
    // drags the shared node - and with it B's start vertex - along.
    private const int SegmentAId = 1;
    private const int SegmentBId = 2;

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

    private ScopedRoadNetwork BuildNetwork(RoadNodeWasAdded[] nodes, RoadSegmentWasAdded[] segments)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            nodes.Select(x => RoadNode.Create(x).WithoutChanges()).ToArray(),
            segments.Select(x => RoadSegment.Create(x).WithoutChanges()).ToArray(),
            [],
            []).WithoutChanges();
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

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentChangeGeometryMeasuredNotAllowed");
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

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentChangeGeometryNotFound");
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
