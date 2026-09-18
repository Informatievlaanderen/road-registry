namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.MigrateGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class ScopedRoadNetworkTests : RoadNetworkTestBase
{
    private static readonly GradeSeparatedJunctionId JunctionId = new(1);

    private readonly Point _segment1Start = new(0, 0);
    private readonly Point _segment1End = new(10, 0);
    private readonly Point _segment2Start = new(5, 5);
    private readonly Point _segment2End = new(5, -5);

    // An inwinning that leaves a grade separated junction untouched still takes it over: it keeps its identifier and
    // becomes V2, together with the road segments it crosses.
    [Fact]
    public void WhenV1GradeSeparatedJunctionIsModified_ThenMigrated()
    {
        var roadNetwork = BuildV1RoadNetwork();

        var result = roadNetwork.Migrate(MigrateRoadNodesAndRoadSegments()
                .Add(new ModifyGradeSeparatedJunctionChange
                {
                    GradeSeparatedJunctionId = JunctionId,
                    LowerRoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    UpperRoadSegmentId = TestData.Segment2Added.RoadSegmentId,
                    Type = GradeSeparatedJunctionTypeV2.Brug
                }),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        result.Problems.Should().HaveNoError();
        result.Summary.GradeSeparatedJunctions.Modified.Should().Contain(JunctionId);
        result.Summary.GradeSeparatedJunctions.Added.Should().BeEmpty();
        result.Summary.GradeSeparatedJunctions.Removed.Should().BeEmpty();

        var junction = roadNetwork.GradeSeparatedJunctions[JunctionId];
        junction.HasMigrated().Should().BeTrue();
        junction.IsRemoved.Should().BeFalse();

        var junctionMigrated = junction.GetChanges().OfType<GradeSeparatedJunctionWasMigrated>().Should().ContainSingle().Which;
        junctionMigrated.GradeSeparatedJunctionId.Should().Be(JunctionId);
        junctionMigrated.LowerRoadSegmentId.Should().Be(TestData.Segment1Added.RoadSegmentId);
        junctionMigrated.UpperRoadSegmentId.Should().Be(TestData.Segment2Added.RoadSegmentId);
        junctionMigrated.Type.Should().Be(GradeSeparatedJunctionTypeV2.Brug);

        // The crossing did not move, but the geometry still has to follow the migration.
        var changes = junction.GetChanges().ToList();
        changes.Should().HaveCount(2);
        changes[0].Should().BeOfType<GradeSeparatedJunctionWasMigrated>();
        changes[1].Should().BeOfType<GradeSeparatedJunctionGeometryWasChanged>()
            .Which.Geometry.Should().Be(new Point(5, 0) { SRID = WellknownSrids.Lambert08 }.ToJunctionGeometry());
    }

    [Fact]
    public void WhenV1GradeSeparatedJunctionIsModifiedAndItsCrossingMoved_ThenOneGeometryChangeFollowsMigration()
    {
        var roadNetwork = BuildV1RoadNetwork(GradeSeparatedJunction.CreateForMigration(JunctionId, TestData.Segment1Added.RoadSegmentId, TestData.Segment2Added.RoadSegmentId, new Point(4, 0) { SRID = WellknownSrids.Lambert08 }.ToJunctionGeometry()));

        var result = roadNetwork.Migrate(MigrateRoadNodesAndRoadSegments()
                .Add(new ModifyGradeSeparatedJunctionChange
                {
                    GradeSeparatedJunctionId = JunctionId,
                    LowerRoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    UpperRoadSegmentId = TestData.Segment2Added.RoadSegmentId,
                    Type = GradeSeparatedJunctionTypeV2.Brug
                }),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        result.Problems.Should().HaveNoError();

        var changes = roadNetwork.GradeSeparatedJunctions[JunctionId].GetChanges().ToList();
        changes.Should().HaveCount(2);
        changes[0].Should().BeOfType<GradeSeparatedJunctionWasMigrated>();
        changes[1].Should().BeOfType<GradeSeparatedJunctionGeometryWasChanged>()
            .Which.Geometry.Should().Be(new Point(5, 0) { SRID = WellknownSrids.Lambert08 }.ToJunctionGeometry());
    }

    [Fact]
    public void WhenV1GradeSeparatedJunctionIsModifiedWithoutRoadSegments_ThenInvalidOperation()
    {
        var roadNetwork = BuildV1RoadNetwork();

        var act = () => roadNetwork.Migrate(MigrateRoadNodesAndRoadSegments()
                .Add(new ModifyGradeSeparatedJunctionChange
                {
                    GradeSeparatedJunctionId = JunctionId,
                    Type = GradeSeparatedJunctionTypeV2.Tunnel
                }),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("LowerRoadSegmentId is required*");
    }

    [Fact]
    public void WhenGradeSeparatedJunctionHasMigratedAlready_ThenNothingChanges()
    {
        var roadNetwork = BuildV1RoadNetwork(GradeSeparatedJunction.Create(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = JunctionId,
            LowerRoadSegmentId = TestData.Segment1Added.RoadSegmentId,
            UpperRoadSegmentId = TestData.Segment2Added.RoadSegmentId,
            Type = GradeSeparatedJunctionTypeV2.Brug,
            Geometry = new Point(5, 0) { SRID = WellknownSrids.Lambert08 }.ToJunctionGeometry(),
            Provenance = new(TestData.Provenance)
        }).WithoutChanges());

        var result = roadNetwork.Migrate(MigrateRoadNodesAndRoadSegments()
                .Add(new ModifyGradeSeparatedJunctionChange
                {
                    GradeSeparatedJunctionId = JunctionId,
                    LowerRoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    UpperRoadSegmentId = TestData.Segment2Added.RoadSegmentId,
                    Type = GradeSeparatedJunctionTypeV2.Brug
                }),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        result.Problems.Should().HaveNoError();
        result.Summary.GradeSeparatedJunctions.Modified.Should().NotContain(JunctionId);
        roadNetwork.GradeSeparatedJunctions[JunctionId].GetChanges().Should().BeEmpty();
    }

    private ScopedRoadNetwork BuildV1RoadNetwork(GradeSeparatedJunction? gradeSeparatedJunction = null)
    {
        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes:
            [
                RoadNode.CreateForMigration(TestData.Segment1StartNodeAdded.RoadNodeId, _segment1Start.ToRoadNodeGeometry()),
                RoadNode.CreateForMigration(TestData.Segment1EndNodeAdded.RoadNodeId, _segment1End.ToRoadNodeGeometry()),
                RoadNode.CreateForMigration(TestData.Segment2StartNodeAdded.RoadNodeId, _segment2Start.ToRoadNodeGeometry()),
                RoadNode.CreateForMigration(TestData.Segment2EndNodeAdded.RoadNodeId, _segment2End.ToRoadNodeGeometry())
            ],
            roadSegments:
            [
                RoadSegment.CreateForMigration(TestData.Segment1Added.RoadSegmentId, BuildRoadSegmentGeometry(_segment1Start, _segment1End), TestData.Segment1Added.Status, TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.RoadNodeId),
                RoadSegment.CreateForMigration(TestData.Segment2Added.RoadSegmentId, BuildRoadSegmentGeometry(_segment2Start, _segment2End), TestData.Segment2Added.Status, TestData.Segment2StartNodeAdded.RoadNodeId, TestData.Segment2EndNodeAdded.RoadNodeId)
            ],
            gradeSeparatedJunctions:
            [
                gradeSeparatedJunction ?? GradeSeparatedJunction.CreateForMigration(JunctionId, TestData.Segment1Added.RoadSegmentId, TestData.Segment2Added.RoadSegmentId, new Point(5, 0) { SRID = WellknownSrids.Lambert08 }.ToJunctionGeometry())
            ]);
    }

    private RoadNetworkChanges MigrateRoadNodesAndRoadSegments()
    {
        return RoadNetworkChanges.Start()
            .WithProvenance(new FakeProvenance())
            .Add(MigrateRoadNode(TestData.Segment1StartNodeAdded.RoadNodeId, _segment1Start))
            .Add(MigrateRoadNode(TestData.Segment1EndNodeAdded.RoadNodeId, _segment1End))
            .Add(MigrateRoadNode(TestData.Segment2StartNodeAdded.RoadNodeId, _segment2Start))
            .Add(MigrateRoadNode(TestData.Segment2EndNodeAdded.RoadNodeId, _segment2End))
            .Add(MigrateRoadSegment(TestData.AddSegment1, TestData.Segment1Added.RoadSegmentId, BuildRoadSegmentGeometry(_segment1Start, _segment1End)))
            .Add(MigrateRoadSegment(TestData.AddSegment2, TestData.Segment2Added.RoadSegmentId, BuildRoadSegmentGeometry(_segment2Start, _segment2End)));
    }

    private static ModifyRoadNodeChange MigrateRoadNode(RoadNodeId roadNodeId, Point geometry)
    {
        return new ModifyRoadNodeChange
        {
            RoadNodeId = roadNodeId,
            Geometry = geometry.ToRoadNodeGeometry(),
            Grensknoop = false
        };
    }

    private static ModifyRoadSegmentChange MigrateRoadSegment(AddRoadSegmentChange attributes, RoadSegmentId roadSegmentId, RoadSegmentGeometry geometry)
    {
        return new ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(roadSegmentId),
            Geometry = geometry,
            GeometryDrawMethod = attributes.GeometryDrawMethod,
            AccessRestriction = attributes.AccessRestriction,
            Category = attributes.Category,
            Morphology = attributes.Morphology,
            Status = attributes.Status,
            StreetNameId = attributes.StreetNameId,
            MaintenanceAuthorityId = attributes.MaintenanceAuthorityId,
            SurfaceType = attributes.SurfaceType,
            CarTrafficDirection = attributes.CarTrafficDirection,
            BikeTrafficDirection = attributes.BikeTrafficDirection,
            PedestrianTrafficDirection = attributes.PedestrianTrafficDirection
        }.WithDynamicAttributePositionsOnEntireGeometryLength();
    }
}
