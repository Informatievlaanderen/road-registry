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

// GAWR-7824 follow-up. Both municipalities took their extract before either of them uploaded, so both still have the
// whole chain of three roads. The northern one uploads first and collects the top two, which merges them and does away
// with the road node between them. The southern one then uploads the bottom two - and the middle road it brings is one
// its neighbour has already collected.
//
//   north (y=400)   --- the far end of what the neighbour collected
//     | ws-100
//   middle (y=100)  --- the road node the neighbour's merge removed
//     | ws-200
//   border (y=50)   --- the road node both uploads share
//     | ws-300
//   south (y=0)     --- the far end of what this upload collects
//
// The first upload brings ws-100 and ws-200, the second ws-200 and ws-300. What the second one should end up with is
// one road from north to south: ws-200 was already taken, ws-300 is collected, and the two meet at the border road
// node that is no longer needed. The road node the first upload removed stays removed.
public class NeighbourMergeSecondUploadTests : RoadNetworkTestBase
{
    private static readonly Coordinate North = new(0, 400);
    private static readonly Coordinate Middle = new(0, 100);
    private static readonly Coordinate Border = new(0, 50);
    private static readonly Coordinate South = new(0, 0);

    private static readonly RoadNodeId NorthNodeId = new(1);
    private static readonly RoadNodeId MiddleNodeId = new(2);
    private static readonly RoadNodeId BorderNodeId = new(3);
    private static readonly RoadNodeId SouthNodeId = new(4);

    private static readonly RoadSegmentId NorthSegmentId = new(100);
    private static readonly RoadSegmentId MiddleSegmentId = new(200);
    private static readonly RoadSegmentId SouthSegmentId = new(300);

    // What the first upload does, which is the state the second one runs into. Pinned on its own so a failure further
    // down says which of the two uploads went wrong.
    [Fact]
    public void WhenTheNeighbourUploadsFirst_ThenItMergesTheTopTwoAndRemovesTheRoadNodeBetweenThem()
    {
        var roadNetwork = BuildUncollectedRoadNetwork();

        var result = roadNetwork.Migrate(UploadOfTheNeighbour(), null, IdGenerator());

        result.Problems.Should().BeEmpty();

        // The longest of the two keeps the identifier: 300 of the 350 metres is over the 70% the register asks for.
        var merged = roadNetwork.RoadSegments[NorthSegmentId];
        merged.IsRemoved.Should().BeFalse();
        merged.Geometry.Value.Length.Should().BeApproximately(350, 0.01);

        roadNetwork.RoadSegments[MiddleSegmentId].Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);
        roadNetwork.RoadNodes[MiddleNodeId].IsRemoved.Should().BeTrue();

        // The border road node stays: the road south of it has not been collected, so it carries no attributes and
        // the road network cannot tell whether the two may become one.
        roadNetwork.RoadNodes[BorderNodeId].IsRemoved.Should().BeFalse();
    }

    [Fact]
    public void WhenTheSecondUploadBringsTheRoadTheNeighbourAlreadyTook_ThenItIsCollectedWithoutProblems()
    {
        var roadNetwork = BuildUncollectedRoadNetwork();
        roadNetwork.Migrate(UploadOfTheNeighbour(), null, IdGenerator()).Problems.Should().BeEmpty();

        var result = roadNetwork.Migrate(UploadOfThisMunicipality(), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().BeEmpty();
    }

    [Fact]
    public void WhenTheSecondUploadBringsTheRoadTheNeighbourAlreadyTook_ThenTheRoadIsOneWholeFromNorthToSouth()
    {
        var roadNetwork = BuildUncollectedRoadNetwork();
        roadNetwork.Migrate(UploadOfTheNeighbour(), null, IdGenerator()).Problems.Should().BeEmpty();

        roadNetwork.Migrate(UploadOfThisMunicipality(), null, IdGenerator());

        var remaining = roadNetwork.RoadSegments.Values
            .Where(x => !x.IsRemoved && x.Status == RoadSegmentStatusV2.Gerealiseerd)
            .Should().ContainSingle().Subject;
        remaining.Geometry.Value.Length.Should().BeApproximately(400, 0.01);

        // The road node the neighbour removed is not brought back by this, and the one both uploads shared is no
        // longer needed once the two roads are one.
        roadNetwork.RoadNodes[MiddleNodeId].IsRemoved.Should().BeTrue();
        roadNetwork.RoadNodes[BorderNodeId].IsRemoved.Should().BeTrue();

        // The road node the neighbour removed is not brought back by this.
        roadNetwork.RoadNodes[MiddleNodeId].IsRemoved.Should().BeTrue();
        roadNetwork.RoadNodes[BorderNodeId].IsRemoved.Should().BeTrue();
    }

    // The chain of three as both municipalities found it in their extract: still V1, nothing collected yet.
    private ScopedRoadNetwork BuildUncollectedRoadNetwork()
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes:
            [
                RoadNode.CreateForMigration(NorthNodeId, Node(North)),
                RoadNode.CreateForMigration(MiddleNodeId, Node(Middle)),
                RoadNode.CreateForMigration(BorderNodeId, Node(Border)),
                RoadNode.CreateForMigration(SouthNodeId, Node(South))
            ],
            roadSegments:
            [
                RoadSegment.CreateForMigration(NorthSegmentId, Line(North, Middle).ToRoadSegmentGeometry(), RoadSegmentStatusV2.Gerealiseerd, NorthNodeId, MiddleNodeId),
                RoadSegment.CreateForMigration(MiddleSegmentId, Line(Middle, Border).ToRoadSegmentGeometry(), RoadSegmentStatusV2.Gerealiseerd, MiddleNodeId, BorderNodeId),
                RoadSegment.CreateForMigration(SouthSegmentId, Line(Border, South).ToRoadSegmentGeometry(), RoadSegmentStatusV2.Gerealiseerd, BorderNodeId, SouthNodeId)
            ]).WithoutChanges();
    }

    // The northern municipality: the top two roads and the three road nodes they hang off.
    private RoadNetworkChanges UploadOfTheNeighbour()
    {
        return Upload(
            [(NorthNodeId, North), (MiddleNodeId, Middle), (BorderNodeId, Border)],
            [(NorthSegmentId, North, Middle), (MiddleSegmentId, Middle, Border)]);
    }

    // The southern municipality, whose extract predates all of that: the bottom two roads, the middle one of which its
    // neighbour has meanwhile collected, and the road node that is meanwhile gone.
    private RoadNetworkChanges UploadOfThisMunicipality()
    {
        return Upload(
            [(MiddleNodeId, Middle), (BorderNodeId, Border), (SouthNodeId, South)],
            [(MiddleSegmentId, Middle, Border), (SouthSegmentId, Border, South)]);
    }

    private RoadNetworkChanges Upload(
        (RoadNodeId Id, Coordinate Coordinate)[] roadNodes,
        (RoadSegmentId Id, Coordinate From, Coordinate To)[] roadSegments)
    {
        var changes = RoadNetworkChanges.Start().WithProvenance(new FakeProvenance());

        foreach (var (roadNodeId, coordinate) in roadNodes)
        {
            changes = changes.Add(new ModifyRoadNodeChange
            {
                RoadNodeId = roadNodeId,
                Geometry = Node(coordinate),
                Grensknoop = false
            });
        }

        foreach (var (roadSegmentId, from, to) in roadSegments)
        {
            var geometry = Line(from, to);

            changes = changes.Add(new ModifyRoadSegmentChange
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
            });
        }

        return changes;
    }

    // One value over the whole road, so two roads that meet can be merged without their attributes standing in the way.
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
