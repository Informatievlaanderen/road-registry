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

// An inwinning is read in on the centimetre - RoundToCm, all through the DomainV2 feature compare - while the
// register only holds a road node on the centimetre once something has migrated it. A road node that is still as V1
// left it carries every digit of the double it was converted into.
//
// Within a municipality that never shows: the inwinning collects its own road nodes, so the same upload puts them
// on the centimetre before anything is compared against them. Across a border it does show. The road node there
// belongs to the neighbour and is not collected, so a rounded coordinate is compared against an unrounded one, and
// the road segment running up to it is refused for a road node that is plainly there:
//
//   "De eind wegknoop van het wegsegment met WS_TEMPID (...) ontbreekt."
//
// The coordinates below are the ones that produced it, taken off the register as it stands.
public class UnroundedRoadNodeTests : RoadNetworkTestBase
{
    // What V1 left behind, and what an inwinning makes of the same point. Three millimetres apart.
    private static readonly Coordinate AsV1LeftIt = new(599194.3321981372, 681119.6221813029);
    private static readonly Coordinate OnTheCentimetre = new(599194.33, 681119.62);

    private static readonly Coordinate OwnSide = new(599276.45, 680787.45);

    private static readonly RoadNodeId NeighbourNodeId = new(1);
    private static readonly RoadNodeId OwnNodeId = new(2);
    private static readonly RoadSegmentId UploadedSegmentId = new(100);

    [Fact]
    public void WhenTheRoadNodeIsStillWhereV1LeftIt_ThenTheUploadStillFindsIt()
    {
        var roadNetwork = BuildNetwork();

        var result = roadNetwork.Migrate(Upload(), null, IdGenerator());

        result.Problems.Should().BeEmpty();

        var collected = roadNetwork.RoadSegments[UploadedSegmentId];
        collected.StartNodeId.Should().Be(OwnNodeId);
        collected.EndNodeId.Should().Be(NeighbourNodeId);
    }

    // The road node keeps the coordinate it has. This upload does not collect it - it is the neighbour's - so it is
    // not for this upload to move it, and a lookup has no business rewriting what it looked at. Worth pinning:
    // rounding the geometries instead of the coordinates does exactly that, because RoundToCm on a Geometry rewrites
    // its coordinates in place.
    [Fact]
    public void WhenTheRoadNodeIsStillWhereV1LeftIt_ThenMatchingItLeavesItWhereItIs()
    {
        var roadNetwork = BuildNetwork();

        roadNetwork.Migrate(Upload(), null, IdGenerator()).Problems.Should().BeEmpty();

        roadNetwork.RoadNodes[NeighbourNodeId].Geometry.Value.Coordinate
            .Should().Be(AsV1LeftIt);
    }

    private ScopedRoadNetwork BuildNetwork()
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes:
            [
                RoadNode.CreateForMigration(NeighbourNodeId, Node(AsV1LeftIt)),
                RoadNode.CreateForMigration(OwnNodeId, Node(OwnSide))
            ],
            roadSegments:
            [
                RoadSegment.CreateForMigration(
                    UploadedSegmentId,
                    Line(OwnSide, AsV1LeftIt).ToRoadSegmentGeometry(),
                    RoadSegmentStatusV2.Gerealiseerd,
                    OwnNodeId,
                    NeighbourNodeId)
            ]);
    }

    // What the municipality uploads: its own road node and the road running up to its neighbour's, both on the
    // centimetre. The neighbour's road node is not among them - it lies on the other side of the border.
    private RoadNetworkChanges Upload()
    {
        var geometry = Line(OwnSide, OnTheCentimetre);

        return RoadNetworkChanges.Start()
            .WithProvenance(new FakeProvenance())
            .Add(new ModifyRoadNodeChange
            {
                RoadNodeId = OwnNodeId,
                Geometry = Node(OwnSide),
                Grensknoop = false
            })
            .Add(new ModifyRoadSegmentChange
            {
                RoadSegmentIdReference = new RoadSegmentIdReference(UploadedSegmentId),
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
            });
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

    private static InMemoryRoadNetworkIdGenerator IdGenerator()
    {
        return new InMemoryRoadNetworkIdGenerator(initialValue: 1000);
    }

    private static RoadNodeGeometry Node(Coordinate coordinate)
    {
        return RoadNodeGeometry.Create(new Point(coordinate) { SRID = WellknownSrids.Lambert08 });
    }
}
