namespace RoadRegistry.Tests.AggregateTests.RoadNode.RemoveRoadNodeByMergingRoadSegments;

using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

// 'Verwijder wegknoop', which is how road segments are merged: a node that does not belong there leaves roads that
// should be one, and removing it is the same act as joining them.
public class AggregateTests : AggregateTestBase
{
    private const int MiddleNodeId = 11;
    private const int SouthNodeId = 10;
    private const int NorthNodeId = 12;
    private const int EastNodeId = 13;
    private const int WestNodeId = 14;

    private const int SouthSegmentId = 1;
    private const int NorthSegmentId = 2;
    private const int EastSegmentId = 3;
    private const int WestSegmentId = 4;

    private RoadNodeWasAdded BuildNode(int id, double x, double y, RoadNodeTypeV2 type)
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

    // Every attribute value covers the whole segment, so two of them can be joined without their attributes standing
    // in the way.
    private RoadSegmentWasAdded BuildSegment(
        int id,
        RoadNodeWasAdded startNode,
        RoadNodeWasAdded endNode,
        RoadSegmentGeometry geometry,
        RoadSegmentGeometryDrawMethodV2? drawMethod = null,
        RoadSegmentTrafficDirection? carTrafficDirection = null,
        RoadSegmentTrafficDirection? bikeTrafficDirection = null)
    {
        var template = TestData.Segment1Added;

        return template with
        {
            RoadSegmentId = new RoadSegmentId(id),
            StartNodeId = startNode.RoadNodeId,
            EndNodeId = endNode.RoadNodeId,
            Geometry = geometry,
            GeometryDrawMethod = drawMethod ?? RoadSegmentGeometryDrawMethodV2.Ingemeten,
            Status = RoadSegmentStatusV2.Gerealiseerd,
            AccessRestriction = Spanning(template.AccessRestriction, geometry),
            Category = Spanning(template.Category, geometry),
            Morphology = Spanning(template.Morphology, geometry),
            StreetNameId = Spanning(template.StreetNameId, geometry),
            MaintenanceAuthorityId = Spanning(template.MaintenanceAuthorityId, geometry),
            SurfaceType = Spanning(template.SurfaceType, geometry),
            CarTrafficDirection = carTrafficDirection is not null
                ? new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>().Add(carTrafficDirection, geometry)
                : Spanning(template.CarTrafficDirection, geometry),
            BikeTrafficDirection = bikeTrafficDirection is not null
                ? new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>().Add(bikeTrafficDirection, geometry)
                : Spanning(template.BikeTrafficDirection, geometry),
            PedestrianTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>()
                .Add(RoadSegmentPedestrianTrafficDirection.None, geometry),
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

    // A road running north, cut in two at a validatieknoop that is not needed.
    private ScopedRoadNetwork BuildValidatieknoopNetwork(RoadSegmentGeometryDrawMethodV2? northDrawMethod = null)
    {
        var southNode = BuildNode(SouthNodeId, 100, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(MiddleNodeId, 100, 80, RoadNodeTypeV2.Validatieknoop);
        var northNode = BuildNode(NorthNodeId, 100, 160, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [southNode, middleNode, northNode],
            [
                BuildSegment(SouthSegmentId, southNode, middleNode, BuildGeometry((100, 0), (100, 80))),
                BuildSegment(NorthSegmentId, middleNode, northNode, BuildGeometry((100, 80), (100, 160)), northDrawMethod)
            ]);
    }

    // Two roads crossing at an echte knoop that a GRB service placed by mistake: the one carries cars and the other
    // bicycles, so no traffic passes between them and the node holds nothing together.
    private ScopedRoadNetwork BuildEchteKnoopNetwork()
    {
        var southNode = BuildNode(SouthNodeId, 100, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(MiddleNodeId, 100, 80, RoadNodeTypeV2.EchteKnoop);
        var northNode = BuildNode(NorthNodeId, 100, 160, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(EastNodeId, 200, 80, RoadNodeTypeV2.Eindknoop);
        var westNode = BuildNode(WestNodeId, 0, 80, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [southNode, middleNode, northNode, eastNode, westNode],
            [
                BuildSegment(SouthSegmentId, southNode, middleNode, BuildGeometry((100, 0), (100, 80)), carTrafficDirection: RoadSegmentTrafficDirection.Both, bikeTrafficDirection: RoadSegmentTrafficDirection.None),
                BuildSegment(NorthSegmentId, middleNode, northNode, BuildGeometry((100, 80), (100, 160)), carTrafficDirection: RoadSegmentTrafficDirection.Both, bikeTrafficDirection: RoadSegmentTrafficDirection.None),
                BuildSegment(EastSegmentId, middleNode, eastNode, BuildGeometry((100, 80), (200, 80)), carTrafficDirection: RoadSegmentTrafficDirection.None, bikeTrafficDirection: RoadSegmentTrafficDirection.Both),
                BuildSegment(WestSegmentId, westNode, middleNode, BuildGeometry((0, 80), (100, 80)), carTrafficDirection: RoadSegmentTrafficDirection.None, bikeTrafficDirection: RoadSegmentTrafficDirection.Both)
            ]);
    }

    private ScopedRoadNetwork BuildNetworkWith(RoadNode[] roadNodes, params RoadSegment[] roadSegments)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), roadNodes, roadSegments, [], []).WithoutChanges();
    }

    private RoadNetworkChangeResult Act(ScopedRoadNetwork roadNetwork, int roadNodeId = MiddleNodeId)
    {
        return roadNetwork.RemoveRoadNodeByMergingRoadSegments(
            new RoadNodeId(roadNodeId),
            new InMemoryRoadNetworkIdGenerator(initialValue: 100),
            TestData.Provenance);
    }

    [Fact]
    public void WhenTheNodeIsAValidatieknoop_ThenTheTwoRoadSegmentsAreMerged()
    {
        var roadNetwork = BuildValidatieknoopNetwork();

        var result = Act(roadNetwork);

        result.Problems.Should().HaveNoError();

        roadNetwork.RoadSegments.Values.SelectMany(x => x.GetChanges()).OfType<RoadSegmentWasMerged>().Should().ContainSingle();
        roadNetwork.RoadNodes[new RoadNodeId(MiddleNodeId)].IsRemoved.Should().BeTrue();

        var summary = roadNetwork.GetChanges().OfType<RoadNetworkWasChanged>().Should().ContainSingle().Which.Summary;
        summary.RoadNodes.Removed.Should().BeEquivalentTo([MiddleNodeId]);
    }

    // The pairs are the roads across from one another, not the neighbours: the second road you cross walking a circle
    // around the node.
    [Fact]
    public void WhenTheNodeIsAnEchteKnoopWithFourRoadSegments_ThenTheOppositeOnesAreMergedPairwise()
    {
        var roadNetwork = BuildEchteKnoopNetwork();

        var result = Act(roadNetwork);

        result.Problems.Should().HaveNoError();

        roadNetwork.RoadSegments.Values.SelectMany(x => x.GetChanges()).OfType<RoadSegmentWasMerged>().Should().HaveCount(2);
        roadNetwork.RoadNodes[new RoadNodeId(MiddleNodeId)].IsRemoved.Should().BeTrue();

        // The two roads that were kept apart by the node now simply cross, which is a gelijkgrondse kruising.
        roadNetwork.GradeJunctions.Values.Where(x => !x.IsRemoved).Should().ContainSingle();
    }

    // The node is removed once, not once per merged pair.
    [Fact]
    public void WhenFourRoadSegmentsAreMerged_ThenTheNodeIsSummarisedAsRemovedOnce()
    {
        var roadNetwork = BuildEchteKnoopNetwork();

        var result = Act(roadNetwork);

        result.Problems.Should().HaveNoError();

        var summary = roadNetwork.GetChanges().OfType<RoadNetworkWasChanged>().Should().ContainSingle().Which.Summary;
        summary.RoadNodes.Removed.Should().BeEquivalentTo([MiddleNodeId]);
    }

    // VAL-2
    [Fact]
    public void WhenTheRoadNodeDoesNotExist_ThenItIsReported()
    {
        var roadNetwork = BuildValidatieknoopNetwork();

        var result = Act(roadNetwork, roadNodeId: 999);

        result.Problems.Should().Contain(x => x.Reason == "RoadNodeRemoveDoesNotExist");
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // VAL-3
    [Fact]
    public void WhenTheRoadNodeIsRemoved_ThenItIsReported()
    {
        var roadNetwork = BuildValidatieknoopNetwork();
        roadNetwork.RoadNodes[new RoadNodeId(MiddleNodeId)].Remove(TestData.Provenance);

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "RoadNodeRemoveIsRemoved");
    }

    // VAL-4: an eindknoop holds one road, which has nothing to become.
    [Fact]
    public void WhenTheRoadNodeIsNotOneThatCanBeUndone_ThenItIsReported()
    {
        var roadNetwork = BuildValidatieknoopNetwork();

        var result = Act(roadNetwork, roadNodeId: SouthNodeId);

        result.Problems.Should().Contain(x => x.Reason == "RoadNodeCannotBeRemovedByMerging");
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // A road node that has not completed its inwinning is still a V1 node: it carries no type, so there is no saying
    // whether it is one this action may undo at all.
    [Fact]
    public void WhenTheRoadNodeHasNotCompletedItsInwinning_ThenItIsReported()
    {
        var southNode = RoadNode.Create(BuildNode(SouthNodeId, 100, 0, RoadNodeTypeV2.Eindknoop)).WithoutChanges();
        var northNode = RoadNode.Create(BuildNode(NorthNodeId, 100, 160, RoadNodeTypeV2.Eindknoop)).WithoutChanges();
        var middleNode = RoadNode.CreateForMigration(
            new RoadNodeId(MiddleNodeId),
            new Point(new Coordinate(100, 80)) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry());

        var roadNetwork = BuildNetworkWith([southNode, middleNode, northNode]);

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "RoadNodeNotCompletedInwinning");
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // The roads that are to become one carry the attributes the merged road inherits, so a road whose inwinning is not
    // finished has nothing to give it.
    [Fact]
    public void WhenARoadSegmentHasNotCompletedItsInwinning_ThenItIsReported()
    {
        var southNodeAdded = BuildNode(SouthNodeId, 100, 0, RoadNodeTypeV2.Eindknoop);
        var middleNodeAdded = BuildNode(MiddleNodeId, 100, 80, RoadNodeTypeV2.Validatieknoop);
        var northNodeAdded = BuildNode(NorthNodeId, 100, 160, RoadNodeTypeV2.Eindknoop);

        var southSegment = RoadSegment.Create(BuildSegment(SouthSegmentId, southNodeAdded, middleNodeAdded, BuildGeometry((100, 0), (100, 80)))).WithoutChanges();
        var notMigratedNorthSegment = RoadSegment.CreateForMigration(
            new RoadSegmentId(NorthSegmentId),
            BuildGeometry((100, 80), (100, 160)),
            RoadSegmentStatusV2.Gerealiseerd,
            new RoadNodeId(MiddleNodeId),
            new RoadNodeId(NorthNodeId));

        var roadNetwork = BuildNetworkWith(
            [RoadNode.Create(southNodeAdded).WithoutChanges(), RoadNode.Create(middleNodeAdded).WithoutChanges(), RoadNode.Create(northNodeAdded).WithoutChanges()],
            southSegment,
            notMigratedNorthSegment);

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentNotCompletedInwinning");
        roadNetwork.RoadNodes[new RoadNodeId(MiddleNodeId)].IsRemoved.Should().BeFalse();
    }

    // VAL-5: one road cannot be half measured and half sketched.
    [Fact]
    public void WhenTheRoadSegmentsHaveADifferentGeometryDrawMethod_ThenItIsReported()
    {
        var roadNetwork = BuildValidatieknoopNetwork(northDrawMethod: RoadSegmentGeometryDrawMethodV2.Ingeschetst);

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentsMergeGeometryDrawMethodNotEqual");
        roadNetwork.RoadSegments.Values.SelectMany(x => x.GetChanges()).OfType<RoadSegmentWasMerged>().Should().BeEmpty();
        roadNetwork.RoadNodes[new RoadNodeId(MiddleNodeId)].IsRemoved.Should().BeFalse();
    }

    // VAL-6: the two roads meet at both ends, so joining them would close a loop onto a single node.
    [Fact]
    public void WhenTheMergeWouldCloseALoop_ThenItIsReported()
    {
        var westNode = BuildNode(WestNodeId, 0, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(MiddleNodeId, 100, 0, RoadNodeTypeV2.Validatieknoop);

        var roadNetwork = BuildNetwork(
            [westNode, middleNode],
            [
                BuildSegment(SouthSegmentId, westNode, middleNode, BuildGeometry((0, 0), (50, 50), (100, 0))),
                BuildSegment(NorthSegmentId, middleNode, westNode, BuildGeometry((100, 0), (50, -50), (0, 0)))
            ]);

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentsMergeSameStartEndNode");
        roadNetwork.RoadNodes[new RoadNodeId(MiddleNodeId)].IsRemoved.Should().BeFalse();
    }
}
