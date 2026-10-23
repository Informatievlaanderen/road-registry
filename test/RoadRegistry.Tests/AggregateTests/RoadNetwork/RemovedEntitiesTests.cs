namespace RoadRegistry.Tests.AggregateTests.RoadNetwork;

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

// The topology projection does not delete anything for a V2 event, so a road network is loaded with the entities that
// were removed from it still in place. Every action has to account for them itself, and say that they were removed -
// which is a different thing to say than that it could not find them at all. The inwinning upload leans on exactly
// that difference: a road node a neighbouring municipality merged away arrives as removed, not as missing.
//
// When this projection is eventually dropped and the identifiers are read straight off the road network, removed
// entities will be there just the same. These tests are what pins that down.
public class RemovedEntitiesTests : RoadNetworkTestBase
{
    private static readonly RoadNodeId RoadNodeId = new(1);
    private static readonly RoadSegmentId RoadSegmentId = new(100);

    private static readonly Coordinate Start = new(0, 0);
    private static readonly Coordinate End = new(0, 100);

    [Fact]
    public void WhenMigratingARemovedRoadNode_ThenItIsReportedAsRemoved()
    {
        var roadNetwork = BuildNetworkWithRemovedRoadNode();

        var result = roadNetwork.Migrate(Changes(c => c.Add(ModifyRoadNode())), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadNodeIsRemoved");
        result.Problems.Select(x => x.Reason).Should().NotContain("RoadNodeNotFound");
    }

    [Fact]
    public void WhenMigratingARoadNodeTheNetworkNeverHad_ThenItIsReportedAsNotFound()
    {
        // The other half of the distinction: nothing was loaded for this identifier at all.
        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>());

        var result = roadNetwork.Migrate(Changes(c => c.Add(ModifyRoadNode())), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadNodeNotFound");
        result.Problems.Select(x => x.Reason).Should().NotContain("RoadNodeIsRemoved");
    }

    [Fact]
    public void WhenChangingARemovedRoadNode_ThenItIsReportedAsRemoved()
    {
        var roadNetwork = BuildNetworkWithRemovedRoadNode();

        var result = roadNetwork.Change(Changes(c => c.Add(ModifyRoadNode())), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadNodeIsRemoved");
    }

    [Fact]
    public void WhenRemovingARemovedRoadNode_ThenNothingIsSaid()
    {
        // Removing what is already gone is not an error, whichever way it went.
        var roadNetwork = BuildNetworkWithRemovedRoadNode();

        var result = roadNetwork.Change(Changes(c => c.Add(new RemoveRoadNodeChange { RoadNodeId = RoadNodeId })), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        result.Summary.RoadNodes.Removed.Should().BeEmpty();
    }

    [Fact]
    public void WhenMigratingARemovedRoadSegment_ThenItIsReportedAsRemoved()
    {
        var roadNetwork = BuildNetworkWithRemovedRoadSegment();

        var result = roadNetwork.Migrate(Changes(c => c.Add(ModifyRoadSegment())), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadSegmentIsRemoved");
        result.Problems.Select(x => x.Reason).Should().NotContain("RoadSegmentNotFound");
    }

    [Fact]
    public void WhenMigratingARoadSegmentTheNetworkNeverHad_ThenItIsReportedAsNotFound()
    {
        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>());

        var result = roadNetwork.Migrate(Changes(c => c.Add(ModifyRoadSegment())), null, IdGenerator());

        result.Problems.Select(x => x.Reason).Should().Contain("RoadSegmentNotFound");
        result.Problems.Select(x => x.Reason).Should().NotContain("RoadSegmentIsRemoved");
    }

    [Fact]
    public void WhenRemovingARemovedRoadSegment_ThenNothingIsSaid()
    {
        var roadNetwork = BuildNetworkWithRemovedRoadSegment();

        var result = roadNetwork.Change(Changes(c => c.Add(new RemoveRoadSegmentChange { RoadSegmentId = RoadSegmentId })), null, IdGenerator());

        result.Problems.Should().BeEmpty();
        result.Summary.RoadSegments.Removed.Should().BeEmpty();
    }

    [Fact]
    public void WhenARemovedRoadSegmentIsLoaded_ThenItIsOutOfReachOfEverySpatialLookup()
    {
        // The quadtrees are built from what is not removed, which is what keeps a removed entity from being picked up
        // as the road node a geometry ends on, or as a road that something overlaps.
        var roadNetwork = BuildNetworkWithRemovedRoadSegment();

        roadNetwork.GetNonRemovedRoadSegments().Should().BeEmpty();
        roadNetwork.RoadSegments.Should().ContainKey(RoadSegmentId);
    }

    private ScopedRoadNetwork BuildNetworkWithRemovedRoadNode()
    {
        var roadNode = RoadNode.CreateForMigration(RoadNodeId, Node(Start));
        roadNode.Remove(new FakeProvenance()).Should().BeEmpty();

        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes: [roadNode.WithoutChanges()]);
    }

    private ScopedRoadNetwork BuildNetworkWithRemovedRoadSegment()
    {
        var roadSegment = RoadSegment.CreateForMigration(
            RoadSegmentId, Line().ToRoadSegmentGeometry(), RoadSegmentStatusV2.Gerealiseerd, null, null);
        roadSegment.Remove(new FakeProvenance()).Should().BeEmpty();

        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadSegments: [roadSegment.WithoutChanges()]);
    }

    // Add is overloaded per change type, so the caller does the adding.
    private static RoadNetworkChanges Changes(Func<RoadNetworkChanges, RoadNetworkChanges> add)
    {
        return add(RoadNetworkChanges.Start().WithProvenance(new FakeProvenance()));
    }

    private static ModifyRoadNodeChange ModifyRoadNode()
    {
        return new ModifyRoadNodeChange
        {
            RoadNodeId = RoadNodeId,
            Geometry = Node(Start),
            Grensknoop = false
        };
    }

    private ModifyRoadSegmentChange ModifyRoadSegment()
    {
        var geometry = Line();

        return new ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(RoadSegmentId),
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

    private RoadSegmentDynamicAttributeValues<T> Attribute<T>(MultiLineString geometry)
    {
        return new RoadSegmentDynamicAttributeValues<T>()
            .Add(Fixture.Create<T>(), RoadSegmentGeometry.Create(geometry));
    }

    private static MultiLineString Line()
    {
        return new MultiLineString([new LineString([Start, End])]) { SRID = WellknownSrids.Lambert08 };
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
