namespace RoadRegistry.Tests.AggregateTests.RoadNode.RemoveRoadNodeByMergingRoadSegments;

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
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using GradeJunction = RoadRegistry.GradeJunction.GradeJunction;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;
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
    private const int CrossingSegmentId = 5;

    private const int JunctionId = 1;

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

    private ScopedRoadNetwork BuildNetwork(
        RoadNodeWasAdded[] nodes,
        RoadSegmentWasAdded[] segments,
        GradeSeparatedJunction[]? gradeSeparatedJunctions = null,
        GradeJunction[]? gradeJunctions = null)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            nodes.Select(x => RoadNode.Create(x).WithoutChanges()).ToArray(),
            segments.Select(x => RoadSegment.Create(x).WithoutChanges()).ToArray(),
            gradeSeparatedJunctions ?? [],
            gradeJunctions ?? []).WithoutChanges();
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
    private ScopedRoadNetwork BuildEchteKnoopNetwork(
        bool withWestSegment = true,
        RoadSegmentGeometryDrawMethodV2? westDrawMethod = null)
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
                .. withWestSegment
                    ? new[] { BuildSegment(WestSegmentId, westNode, middleNode, BuildGeometry((0, 80), (100, 80)), westDrawMethod, carTrafficDirection: RoadSegmentTrafficDirection.None, bikeTrafficDirection: RoadSegmentTrafficDirection.Both) }
                    : []
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

    // The pairs are the roads across from one another, so a road is joined with the one it continues into - not with
    // either of its neighbours.
    [Fact]
    public void WhenTheNodeIsAnEchteKnoopWithFourRoadSegments_ThenEachRoadIsMergedWithTheOneAcrossFromIt()
    {
        var roadNetwork = BuildEchteKnoopNetwork();

        var result = Act(roadNetwork);

        result.Problems.Should().HaveNoError();
        MergedPairs(roadNetwork, SouthSegmentId, NorthSegmentId, EastSegmentId, WestSegmentId).Should().BeEquivalentTo(new[]
        {
            new[] { SouthSegmentId, NorthSegmentId },
            new[] { EastSegmentId, WestSegmentId }
        });
    }

    // Nothing is at a right angle here: the rule is positional, not a matter of the roads lining up.
    [Fact]
    public void WhenTheEchteKnoopIsSkewed_ThenTheRoadsAcrossFromOneAnotherAreStillPaired()
    {
        var roadNetwork = BuildSkewedEchteKnoopNetwork();

        var result = Act(roadNetwork);

        result.Problems.Should().HaveNoError();
        MergedPairs(roadNetwork, SouthSegmentId, NorthSegmentId, EastSegmentId, WestSegmentId).Should().BeEquivalentTo(new[]
        {
            new[] { SouthSegmentId, NorthSegmentId },
            new[] { EastSegmentId, WestSegmentId }
        });
    }

    // A road is paired on the direction it leaves the node in, not on where it ends up: the west road leaves due west
    // and then swings round to end up north-east of the node. Judged on its far end it would sort in between east and
    // north, and east would be handed the wrong road to become.
    [Fact]
    public void WhenARoadCurvesAwayFromTheNode_ThenItIsPairedOnHowItLeavesTheNode()
    {
        var roadNetwork = BuildCurvingEchteKnoopNetwork();

        var result = Act(roadNetwork);

        result.Problems.Should().HaveNoError();
        MergedPairs(roadNetwork, SouthSegmentId, NorthSegmentId, EastSegmentId, WestSegmentId).Should().BeEquivalentTo(new[]
        {
            new[] { SouthSegmentId, NorthSegmentId },
            new[] { EastSegmentId, WestSegmentId }
        });
    }

    // VAL-4: an echte knoop holds two pairs or it holds none - three roads have no across.
    [Fact]
    public void WhenTheEchteKnoopDoesNotHoldFourRoadSegments_ThenItIsReported()
    {
        var roadNetwork = BuildEchteKnoopNetwork(withWestSegment: false);

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "RoadNodeCannotBeRemovedByMerging");
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // VAL-4: a validatieknoop holds exactly the two roads that become one.
    [Fact]
    public void WhenTheValidatieknoopDoesNotHoldTwoRoadSegments_ThenItIsReported()
    {
        var westNode = BuildNode(WestNodeId, 0, 80, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(MiddleNodeId, 100, 80, RoadNodeTypeV2.Validatieknoop);
        var eastNode = BuildNode(EastNodeId, 200, 80, RoadNodeTypeV2.Eindknoop);
        var northNode = BuildNode(NorthNodeId, 100, 160, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [westNode, middleNode, eastNode, northNode],
            [
                BuildSegment(WestSegmentId, westNode, middleNode, BuildGeometry((0, 80), (100, 80))),
                BuildSegment(EastSegmentId, middleNode, eastNode, BuildGeometry((100, 80), (200, 80))),
                BuildSegment(NorthSegmentId, middleNode, northNode, BuildGeometry((100, 80), (100, 160)))
            ]);

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "RoadNodeCannotBeRemovedByMerging");
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // VAL-4: a road that begins and ends at this node is already the whole loop, so it has no other road to become -
    // and neither has the road hanging off the same node.
    [Fact]
    public void WhenARoadBeginsAndEndsAtTheNode_ThenItIsReported()
    {
        var middleNode = BuildNode(MiddleNodeId, 100, 80, RoadNodeTypeV2.Validatieknoop);
        var westNode = BuildNode(WestNodeId, 0, 80, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [middleNode, westNode],
            [
                BuildSegment(WestSegmentId, westNode, middleNode, BuildGeometry((0, 80), (100, 80))),
                BuildSegment(NorthSegmentId, middleNode, middleNode, BuildGeometry((100, 80), (150, 130), (50, 130), (100, 80)))
            ]);

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "RoadNodeCannotBeRemovedByMerging");
        roadNetwork.GetChanges().Should().BeEmpty();
    }

    // VAL-7: the merged road would cross itself.
    [Fact]
    public void WhenTheMergedRoadWouldCrossItself_ThenItIsReported()
    {
        var westNode = BuildNode(WestNodeId, 0, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(MiddleNodeId, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var northNode = BuildNode(NorthNodeId, 50, 50, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [westNode, middleNode, northNode],
            [
                BuildSegment(WestSegmentId, westNode, middleNode, BuildGeometry((0, 0), (100, 0))),
                BuildSegment(NorthSegmentId, middleNode, northNode, BuildGeometry((100, 0), (50, -50), (50, 50)))
            ]);

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentsMergeSelfIntersecting");
        roadNetwork.RoadNodes[new RoadNodeId(MiddleNodeId)].IsRemoved.Should().BeFalse();
    }

    // VAL-8: the merged road would run into another road more than once, which is a crossing the network cannot
    // resolve into one grade separated junction.
    [Fact]
    public void WhenTheMergedRoadWouldRunIntoAnotherRoadTwice_ThenItIsReported()
    {
        var southNode = BuildNode(SouthNodeId, 100, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(MiddleNodeId, 100, 80, RoadNodeTypeV2.Validatieknoop);
        var northNode = BuildNode(NorthNodeId, 100, 160, RoadNodeTypeV2.Eindknoop);
        var westNode = BuildNode(WestNodeId, 50, 20, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(EastNodeId, 50, 140, RoadNodeTypeV2.Eindknoop);

        var roadNetwork = BuildNetwork(
            [southNode, middleNode, northNode, westNode, eastNode],
            [
                BuildSegment(SouthSegmentId, southNode, middleNode, BuildGeometry((100, 0), (100, 80))),
                BuildSegment(NorthSegmentId, middleNode, northNode, BuildGeometry((100, 80), (100, 160))),
                BuildSegment(WestSegmentId, westNode, eastNode, BuildGeometry((50, 20), (150, 20), (150, 140), (50, 140)))
            ]);

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentsMergeMultipleIntersections");
        roadNetwork.RoadNodes[new RoadNodeId(MiddleNodeId)].IsRemoved.Should().BeFalse();
    }

    // Merging four roads only succeeds if both pairs can be merged, so a problem on one pair leaves the other pair -
    // and the node - untouched.
    [Fact]
    public void WhenOnePairOfTheEchteKnoopCannotBeMerged_ThenNoRoadIsMerged()
    {
        var roadNetwork = BuildEchteKnoopNetwork(westDrawMethod: RoadSegmentGeometryDrawMethodV2.Ingeschetst);

        var result = Act(roadNetwork);

        result.Problems.Should().Contain(x => x.Reason == "RoadSegmentsMergeGeometryDrawMethodNotEqual");
        roadNetwork.RoadSegments.Values.SelectMany(x => x.GetChanges()).OfType<RoadSegmentWasMerged>().Should().BeEmpty();
        roadNetwork.RoadNodes[new RoadNodeId(MiddleNodeId)].IsRemoved.Should().BeFalse();
    }

    // Which roads ended up joined, as unordered pairs of the identifiers they had before the merge.
    //
    // Two roads that become one either continue under the identifier of the longest of the two - the other is retired
    // into it - or, when neither is long enough to keep it, both are retired into a newly added road. Grouping the
    // roads by what they became covers both.
    private static int[][] MergedPairs(ScopedRoadNetwork roadNetwork, params int[] originalRoadSegmentIds)
    {
        var retiredInto = roadNetwork.RoadSegments.Values
            .SelectMany(x => x.GetChanges())
            .OfType<RoadSegmentWasRetiredBecauseOfMerger>()
            .ToDictionary(x => (int)x.RoadSegmentId, x => (int)x.MergedRoadSegmentId);

        return originalRoadSegmentIds
            .GroupBy(id => retiredInto.TryGetValue(id, out var mergedId) ? mergedId : id)
            .Select(x => x.OrderBy(id => id).ToArray())
            .OrderBy(x => x[0])
            .ToArray();
    }

    // The same four roads, but leaving the node at 0, 53, 143 and 307 degrees instead of at right angles. Every leg
    // is a 3-4-5 triangle so that the merged roads still come out at a whole number of metres.
    private ScopedRoadNetwork BuildSkewedEchteKnoopNetwork()
    {
        var middleNode = BuildNode(MiddleNodeId, 100, 80, RoadNodeTypeV2.EchteKnoop);
        var eastNode = BuildNode(EastNodeId, 180, 80, RoadNodeTypeV2.Eindknoop);
        var northNode = BuildNode(NorthNodeId, 148, 144, RoadNodeTypeV2.Eindknoop);
        var westNode = BuildNode(WestNodeId, 36, 128, RoadNodeTypeV2.Eindknoop);
        var southNode = BuildNode(SouthNodeId, 148, 16, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [southNode, middleNode, northNode, eastNode, westNode],
            [
                // 307 and 53 degrees: the pair that runs from the south-east to the north-east.
                BuildSegment(SouthSegmentId, southNode, middleNode, BuildGeometry((148, 16), (100, 80)), carTrafficDirection: RoadSegmentTrafficDirection.Both, bikeTrafficDirection: RoadSegmentTrafficDirection.None),
                BuildSegment(NorthSegmentId, middleNode, northNode, BuildGeometry((100, 80), (148, 144)), carTrafficDirection: RoadSegmentTrafficDirection.Both, bikeTrafficDirection: RoadSegmentTrafficDirection.None),
                // 0 and 143 degrees: the pair that runs from the east to the north-west.
                BuildSegment(EastSegmentId, middleNode, eastNode, BuildGeometry((100, 80), (180, 80)), carTrafficDirection: RoadSegmentTrafficDirection.None, bikeTrafficDirection: RoadSegmentTrafficDirection.Both),
                BuildSegment(WestSegmentId, westNode, middleNode, BuildGeometry((36, 128), (100, 80)), carTrafficDirection: RoadSegmentTrafficDirection.None, bikeTrafficDirection: RoadSegmentTrafficDirection.Both)
            ]);
    }

    // The west road leaves the node due west and then swings round to end north-east of it.
    private ScopedRoadNetwork BuildCurvingEchteKnoopNetwork()
    {
        var southNode = BuildNode(SouthNodeId, 100, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(MiddleNodeId, 100, 80, RoadNodeTypeV2.EchteKnoop);
        var northNode = BuildNode(NorthNodeId, 100, 100, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(EastNodeId, 200, 80, RoadNodeTypeV2.Eindknoop);
        var westNode = BuildNode(WestNodeId, 104, 125, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [southNode, middleNode, northNode, eastNode, westNode],
            [
                BuildSegment(SouthSegmentId, southNode, middleNode, BuildGeometry((100, 0), (100, 80)), carTrafficDirection: RoadSegmentTrafficDirection.Both, bikeTrafficDirection: RoadSegmentTrafficDirection.None),
                BuildSegment(NorthSegmentId, middleNode, northNode, BuildGeometry((100, 80), (100, 100)), carTrafficDirection: RoadSegmentTrafficDirection.Both, bikeTrafficDirection: RoadSegmentTrafficDirection.None),
                BuildSegment(EastSegmentId, middleNode, eastNode, BuildGeometry((100, 80), (200, 80)), carTrafficDirection: RoadSegmentTrafficDirection.None, bikeTrafficDirection: RoadSegmentTrafficDirection.Both),
                BuildSegment(WestSegmentId, middleNode, westNode, BuildGeometry((100, 80), (99, 80), (80, 110), (104, 125)), carTrafficDirection: RoadSegmentTrafficDirection.None, bikeTrafficDirection: RoadSegmentTrafficDirection.Both)
            ]);
    }

    // An ongelijkgrondse kruising that was about one of the two roads is about the road they became, and says so under
    // its own identifier rather than being torn down and put back.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WhenAGradeSeparatedJunctionIsAttachedToOneOfTheMergedRoads_ThenItPointsAtTheRoadTheyBecame(bool theSouthRoadKeepsItsIdentifier)
    {
        var roadNetwork = BuildCrossedValidatieknoopNetwork(
            withGradeSeparatedJunction: true,
            theSouthRoadKeepsItsIdentifier: theSouthRoadKeepsItsIdentifier);

        var result = Act(roadNetwork);

        result.Problems.Should().HaveNoError();

        var mergedRoadSegmentId = MergedInto(roadNetwork, NorthSegmentId);
        mergedRoadSegmentId.Should().Be(new RoadSegmentId(theSouthRoadKeepsItsIdentifier ? SouthSegmentId : 100));

        var junction = roadNetwork.GradeSeparatedJunctions.Values.Should().ContainSingle().Which;
        junction.GradeSeparatedJunctionId.Should().Be(new GradeSeparatedJunctionId(JunctionId),
            "the crossing is the same crossing, so it keeps its identifier");
        junction.IsRemoved.Should().BeFalse();
        junction.UpperRoadSegmentId.Should().Be(mergedRoadSegmentId);
        junction.LowerRoadSegmentId.Should().Be(new RoadSegmentId(CrossingSegmentId));

        junction.GetChanges().OfType<GradeSeparatedJunctionWasModified>().Should().ContainSingle();
    }

    // The same for the crossings at grade.
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WhenAGradeJunctionIsAttachedToOneOfTheMergedRoads_ThenItPointsAtTheRoadTheyBecame(bool theSouthRoadKeepsItsIdentifier)
    {
        var roadNetwork = BuildCrossedValidatieknoopNetwork(
            withGradeSeparatedJunction: false,
            theSouthRoadKeepsItsIdentifier: theSouthRoadKeepsItsIdentifier);

        var result = Act(roadNetwork);

        result.Problems.Should().HaveNoError();

        var mergedRoadSegmentId = MergedInto(roadNetwork, NorthSegmentId);

        var junction = roadNetwork.GradeJunctions.Values.Should().ContainSingle().Which;
        junction.GradeJunctionId.Should().Be(new GradeJunctionId(JunctionId),
            "the crossing is the same crossing, so it keeps its identifier");
        junction.IsRemoved.Should().BeFalse();
        junction.RoadSegmentId1.Should().Be(mergedRoadSegmentId);
        junction.RoadSegmentId2.Should().Be(new RoadSegmentId(CrossingSegmentId));

        junction.GetChanges().OfType<GradeJunctionWasModified>().Should().ContainSingle();
    }

    // The two roads that were kept apart by the node now simply cross, and that crossing is recorded as a
    // gelijkgrondse kruising between the two roads they became.
    [Fact]
    public void WhenFourRoadSegmentsAreMerged_ThenAGradeJunctionIsAddedBetweenTheTwoRoadsTheyBecame()
    {
        var roadNetwork = BuildEchteKnoopNetwork();

        var result = Act(roadNetwork);

        result.Problems.Should().HaveNoError();

        var junction = roadNetwork.GradeJunctions.Values.Where(x => !x.IsRemoved).Should().ContainSingle().Which;
        new[] { junction.RoadSegmentId1, junction.RoadSegmentId2 }.Should().BeEquivalentTo(new[]
        {
            MergedInto(roadNetwork, SouthSegmentId),
            MergedInto(roadNetwork, EastSegmentId)
        });
        junction.GetChanges().OfType<GradeJunctionWasAdded>().Should().ContainSingle();
    }

    // What a road became: the identifier of the longest of the two when it was long enough to keep it, and the
    // identifier of the newly added road when neither was.
    private static RoadSegmentId MergedInto(ScopedRoadNetwork roadNetwork, int originalRoadSegmentId)
    {
        var retired = roadNetwork.RoadSegments.Values
            .SelectMany(x => x.GetChanges())
            .OfType<RoadSegmentWasRetiredBecauseOfMerger>()
            .SingleOrDefault(x => x.RoadSegmentId == new RoadSegmentId(originalRoadSegmentId));

        return retired?.MergedRoadSegmentId ?? new RoadSegmentId(originalRoadSegmentId);
    }

    // The road cut in two at a validatieknoop, with a third road crossing its northern half - and a crossing recorded
    // for it. Merging is expected to leave that crossing be, other than saying which road it is about now.
    //
    // The southern road is either as long as the northern one, in which case neither is long enough to keep its
    // identifier and the merged road is a new one, or long enough that the merged road continues under its identifier.
    private ScopedRoadNetwork BuildCrossedValidatieknoopNetwork(bool withGradeSeparatedJunction, bool theSouthRoadKeepsItsIdentifier)
    {
        var southNode = BuildNode(SouthNodeId, 100, theSouthRoadKeepsItsIdentifier ? -220 : 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(MiddleNodeId, 100, 80, RoadNodeTypeV2.Validatieknoop);
        var northNode = BuildNode(NorthNodeId, 100, 160, RoadNodeTypeV2.Eindknoop);
        var westNode = BuildNode(WestNodeId, 50, 120, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(EastNodeId, 150, 120, RoadNodeTypeV2.Eindknoop);

        var crossing = new Point(new Coordinate(100, 120)) { SRID = WellknownSrids.Lambert08 };

        // An ongelijkgrondse kruising is what a crossing needs when the two roads carry the same kind of traffic; a
        // gelijkgrondse one is only allowed when they do not.
        var crossingRoadBikeTrafficDirection = withGradeSeparatedJunction
            ? RoadSegmentTrafficDirection.None
            : RoadSegmentTrafficDirection.Both;
        var crossingRoadCarTrafficDirection = withGradeSeparatedJunction
            ? RoadSegmentTrafficDirection.Both
            : RoadSegmentTrafficDirection.None;

        return BuildNetwork(
            [southNode, middleNode, northNode, westNode, eastNode],
            [
                BuildSegment(SouthSegmentId, southNode, middleNode, BuildGeometry((100, theSouthRoadKeepsItsIdentifier ? -220 : 0), (100, 80)), carTrafficDirection: RoadSegmentTrafficDirection.Both, bikeTrafficDirection: RoadSegmentTrafficDirection.None),
                BuildSegment(NorthSegmentId, middleNode, northNode, BuildGeometry((100, 80), (100, 160)), carTrafficDirection: RoadSegmentTrafficDirection.Both, bikeTrafficDirection: RoadSegmentTrafficDirection.None),
                BuildSegment(CrossingSegmentId, westNode, eastNode, BuildGeometry((50, 120), (150, 120)), carTrafficDirection: crossingRoadCarTrafficDirection, bikeTrafficDirection: crossingRoadBikeTrafficDirection)
            ],
            gradeSeparatedJunctions: withGradeSeparatedJunction
                ?
                [
                    GradeSeparatedJunction.Create(new GradeSeparatedJunctionWasAdded
                    {
                        GradeSeparatedJunctionId = new GradeSeparatedJunctionId(JunctionId),
                        UpperRoadSegmentId = new RoadSegmentId(NorthSegmentId),
                        LowerRoadSegmentId = new RoadSegmentId(CrossingSegmentId),
                        Type = GradeSeparatedJunctionTypeV2.Brug,
                        Geometry = JunctionGeometry.Create(crossing),
                        Provenance = new ProvenanceData(TestData.Provenance)
                    }).WithoutChanges()
                ]
                : [],
            gradeJunctions: withGradeSeparatedJunction
                ? []
                :
                [
                    GradeJunction.Create(new GradeJunctionWasAdded
                    {
                        GradeJunctionId = new GradeJunctionId(JunctionId),
                        RoadSegmentId1 = new RoadSegmentId(NorthSegmentId),
                        RoadSegmentId2 = new RoadSegmentId(CrossingSegmentId),
                        Geometry = JunctionGeometry.Create(crossing),
                        Provenance = new ProvenanceData(TestData.Provenance)
                    }).WithoutChanges()
                ]);
    }
}
