namespace RoadRegistry.Tests.AggregateTests.RoadSegment.MigrateRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

// A road node this upload names that the road network no longer has is read as one a neighbouring inwinning merged
// away, as soon as a road leaving it could have taken a stretch of the upload over. That road has to be the
// neighbour's: one this upload modifies is its own, and reading it as a takeover cuts the upload back against itself
// and lets go of a road node that is still needed (GAWR-7827).
//
// Three of this municipality's own roads meet at a road node the neighbour removed, which is the ordinary shape of a
// junction on the border:
//
//                        o fork-north (100, 1)
//      ws-202     ws-200 -'
//   o ----------- x border (0,0)
//   west          ws-201 -.
//   (-100,0)              o fork-south (100, -1)
//
// The two to the east leave the road node within a centimetre of one another, which is all the probe that looks for a
// takeover reaches: before the fix one of them was taken for the other's neighbour. The third makes the road node a
// junction the topology keeps, so what became of it can be read off the result.
public class NeighbourMergeOwnRoadsTests : RoadNetworkTestBase
{
    private static readonly Coordinate Border = new(0, 0);
    private static readonly Coordinate ForkNorth = new(100, 1);
    private static readonly Coordinate ForkSouth = new(100, -1);
    private static readonly Coordinate West = new(-100, 0);

    private static readonly RoadNodeId BorderNodeId = new(1);
    private static readonly RoadNodeId ForkNorthNodeId = new(2);
    private static readonly RoadNodeId ForkSouthNodeId = new(3);
    private static readonly RoadNodeId WestNodeId = new(4);

    private static readonly RoadSegmentId NorthSegmentId = new(200);
    private static readonly RoadSegmentId SouthSegmentId = new(201);
    private static readonly RoadSegmentId WestSegmentId = new(202);

    // Nothing here is a neighbour's doing: all three roads are this upload's, so there is no merge to account for, and
    // the road node is brought back by the removal correction instead.
    [Fact]
    public void WhenItsOwnRoadsMeetAtTheRemovedRoadNode_ThenNoneIsReadAsATakeover()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        roadNetwork.RoadNodes[BorderNodeId].IsRemoved.Should().BeFalse();
    }

    // A road cut back against another would no longer reach the road node it was uploaded to run to.
    [Fact]
    public void WhenItsOwnRoadsMeetAtTheRemovedRoadNode_ThenNoneIsCutBack()
    {
        var roadNetwork = BuildNetworkTheNeighbourLeftBehind();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());
        result.Problems.Should().BeEmpty();

        foreach (var roadSegmentId in new[] { NorthSegmentId, SouthSegmentId, WestSegmentId })
        {
            var roadSegment = roadNetwork.RoadSegments[roadSegmentId];
            new[] { roadSegment.StartNodeId, roadSegment.EndNodeId }.Should().Contain(BorderNodeId);
        }
    }

    // The road node on the border, removed by the neighbour, with this municipality's three roads meeting at it.
    private ScopedRoadNetwork BuildNetworkTheNeighbourLeftBehind()
    {
        return BuildNetwork(
            roadNodes: [(ForkNorthNodeId, ForkNorth), (ForkSouthNodeId, ForkSouth), (WestNodeId, West)],
            roadSegments:
            [
                (NorthSegmentId, Border, ForkNorth, BorderNodeId, ForkNorthNodeId),
                (SouthSegmentId, Border, ForkSouth, BorderNodeId, ForkSouthNodeId),
                (WestSegmentId, West, Border, WestNodeId, BorderNodeId)
            ],
            removedRoadNodes: [(BorderNodeId, Border)]);
    }

    private RoadNetworkChanges Upload()
    {
        return RoadNetworkChanges.Start()
            .WithProvenance(new FakeProvenance())
            .Add(new ModifyRoadNodeChange { RoadNodeId = BorderNodeId, Geometry = Node(Border), Grensknoop = false })
            .Add(new ModifyRoadNodeChange { RoadNodeId = ForkNorthNodeId, Geometry = Node(ForkNorth), Grensknoop = false })
            .Add(new ModifyRoadNodeChange { RoadNodeId = ForkSouthNodeId, Geometry = Node(ForkSouth), Grensknoop = false })
            .Add(new ModifyRoadNodeChange { RoadNodeId = WestNodeId, Geometry = Node(West), Grensknoop = false })
            .Add(RoadSegmentChange(NorthSegmentId, Border, ForkNorth))
            .Add(RoadSegmentChange(SouthSegmentId, Border, ForkSouth))
            .Add(RoadSegmentChange(WestSegmentId, West, Border));
    }

    private ModifyRoadSegmentChange RoadSegmentChange(RoadSegmentId roadSegmentId, Coordinate from, Coordinate to)
    {
        var geometry = Line(from, to);

        return new ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(roadSegmentId),
            Geometry = geometry.ToRoadSegmentGeometry(),
            GeometryDrawMethod = RoadSegmentGeometryDrawMethodV2.Ingemeten,
            Status = RoadSegmentStatusV2.Gerealiseerd,
            AccessRestriction = Attribute<RoadSegmentAccessRestrictionV2>(geometry),
            Category = Attribute<RoadSegmentCategoryV2>(geometry),
            Morphology = Attribute<RoadSegmentMorphologyV2>(geometry),
            StreetNameId = Attribute<StreetNameLocalId>(geometry),
            MaintenanceAuthorityId = Attribute<OrganizationId>(geometry),
            SurfaceType = Attribute<RoadSegmentSurfaceTypeV2>(geometry),
            CarTrafficDirection = Attribute<RoadSegmentTrafficDirection>(geometry),
            BikeTrafficDirection = Attribute<RoadSegmentTrafficDirection>(geometry),
            PedestrianTrafficDirection = Attribute<RoadSegmentPedestrianTrafficDirection>(geometry)
        };
    }

    private ScopedRoadNetwork BuildNetwork(
        (RoadNodeId Id, Coordinate Coordinate)[] roadNodes,
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId)[] roadSegments,
        (RoadNodeId Id, Coordinate Coordinate)[]? removedRoadNodes = null)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes: roadNodes
                .Select(x => RoadNode.CreateForMigration(x.Id, Node(x.Coordinate)))
                .Concat((removedRoadNodes ?? []).Select(BuildRemovedRoadNode))
                .ToArray(),
            roadSegments: roadSegments
                .Select(BuildRoadSegment)
                .ToArray());
    }

    private static RoadSegment BuildRoadSegment(
        (RoadSegmentId Id, Coordinate From, Coordinate To, RoadNodeId StartNodeId, RoadNodeId EndNodeId) roadSegment)
    {
        return RoadSegment.CreateForMigration(
            roadSegment.Id,
            Line(roadSegment.From, roadSegment.To).ToRoadSegmentGeometry(),
            RoadSegmentStatusV2.Gerealiseerd,
            roadSegment.StartNodeId,
            roadSegment.EndNodeId);
    }

    private static RoadNode BuildRemovedRoadNode((RoadNodeId Id, Coordinate Coordinate) roadNode)
    {
        var removed = RoadNode.CreateForMigration(roadNode.Id, Node(roadNode.Coordinate));
        removed.RemoveBecauseOfMigration(new FakeProvenance());

        return removed.WithoutChanges();
    }

    private RoadSegmentDynamicAttributeValues<T> Attribute<T>(MultiLineString geometry)
    {
        return new RoadSegmentDynamicAttributeValues<T>()
            .Add(Fixture.Create<T>(), RoadSegmentGeometry.Create(geometry));
    }

    private static MultiLineString Line(Coordinate from, Coordinate to)
    {
        return new MultiLineString([new LineString([from, to])])
        {
            SRID = WellknownSrids.Lambert08
        };
    }

    private static RoadNodeGeometry Node(Coordinate coordinate)
    {
        return new Point(coordinate) { SRID = WellknownSrids.Lambert08 }.ToRoadNodeGeometry();
    }

    private static InMemoryRoadNetworkIdGenerator IdGenerator()
    {
        return new InMemoryRoadNetworkIdGenerator(initialValue: 1000);
    }
}
