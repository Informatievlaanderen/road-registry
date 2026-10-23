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
using RoadRegistry.ValueObjects.Problems;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class ScopedRoadNetworkTests : RoadNetworkTestBase
{
    private static readonly GradeSeparatedJunctionId JunctionId = new(1);
    private static readonly GradeSeparatedJunctionId OtherJunctionId = new(2);

    private readonly Point _segment1Start = new(0, 0);
    private readonly Point _segment1End = new(10, 0);
    private readonly Point _segment2Start = new(5, 5);
    private readonly Point _segment2End = new(5, -5);
    private readonly Point _segment3Start = new(100, 100);
    private readonly Point _segment3End = new(110, 100);

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

    // The inwinning that brought this to light. V1 allowed two ongelijkgrondse kruisingen on one and the same segment
    // pair, and the migration does not correct that - so the scope loaded for an upload can hold a pair of them that
    // the upload neither carries nor has to carry, here on two V1 road segments while the upload is about another road
    // segment entirely. Uniqueness is a V2 rule, judged over the kruisingen that have migrated, so the two V1 ones are
    // left alone rather than reported to whoever happens to be uploading.
    [Fact]
    public void WhenRoadSegmentsOutsideTheInwinningCarryTwoV1GradeSeparatedJunctions_ThenNotUniqueIsNotReported()
    {
        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes:
            [
                RoadNode.CreateForMigration(TestData.Segment1StartNodeAdded.RoadNodeId, _segment1Start.ToRoadNodeGeometry()),
                RoadNode.CreateForMigration(TestData.Segment1EndNodeAdded.RoadNodeId, _segment1End.ToRoadNodeGeometry()),
                RoadNode.CreateForMigration(TestData.Segment2StartNodeAdded.RoadNodeId, _segment2Start.ToRoadNodeGeometry()),
                RoadNode.CreateForMigration(TestData.Segment2EndNodeAdded.RoadNodeId, _segment2End.ToRoadNodeGeometry()),
                RoadNode.CreateForMigration(TestData.Segment3StartNodeAdded.RoadNodeId, _segment3Start.ToRoadNodeGeometry()),
                RoadNode.CreateForMigration(TestData.Segment3EndNodeAdded.RoadNodeId, _segment3End.ToRoadNodeGeometry())
            ],
            roadSegments:
            [
                CreateV1RoadSegment(TestData.Segment1Added.RoadSegmentId, _segment1Start, _segment1End, TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.RoadNodeId),
                CreateV1RoadSegment(TestData.Segment2Added.RoadSegmentId, _segment2Start, _segment2End, TestData.Segment2StartNodeAdded.RoadNodeId, TestData.Segment2EndNodeAdded.RoadNodeId),
                CreateV1RoadSegment(TestData.Segment3Added.RoadSegmentId, _segment3Start, _segment3End, TestData.Segment3StartNodeAdded.RoadNodeId, TestData.Segment3EndNodeAdded.RoadNodeId)
            ],
            gradeSeparatedJunctions:
            [
                CreateV1GradeSeparatedJunction(JunctionId),
                CreateV1GradeSeparatedJunction(OtherJunctionId)
            ]);

        // The upload carries road segment 3 and its two road nodes, and nothing of the two crossing V1 segments.
        var result = roadNetwork.Migrate(RoadNetworkChanges.Start()
                .WithProvenance(new FakeProvenance())
                .Add(MigrateRoadNode(TestData.Segment3StartNodeAdded.RoadNodeId, _segment3Start))
                .Add(MigrateRoadNode(TestData.Segment3EndNodeAdded.RoadNodeId, _segment3End))
                .Add(MigrateRoadSegment(TestData.AddSegment3, TestData.Segment3Added.RoadSegmentId, BuildRoadSegmentGeometry(_segment3Start, _segment3End))),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        result.Problems.Should().HaveNoError();
        roadNetwork.RoadSegments[TestData.Segment3Added.RoadSegmentId].HasMigrated().Should().BeTrue();

        // The segments carrying the two kruisingen stay V1, and so do the kruisingen themselves.
        roadNetwork.RoadSegments[TestData.Segment1Added.RoadSegmentId].HasMigrated().Should().BeFalse();
        roadNetwork.RoadSegments[TestData.Segment2Added.RoadSegmentId].HasMigrated().Should().BeFalse();
        foreach (var junctionId in new[] { JunctionId, OtherJunctionId })
        {
            var junction = roadNetwork.GradeSeparatedJunctions[junctionId];
            junction.HasMigrated().Should().BeFalse();
            junction.IsRemoved.Should().BeFalse();
            junction.GetChanges().Should().BeEmpty();
        }
    }

    // A kruising migrates together with the road segments it crosses, so an upload that carries the two segments
    // carries both kruisingen as well - and then the rule does apply: the pair holds two V2 kruisingen.
    [Fact]
    public void WhenBothV1GradeSeparatedJunctionsOnTheSameRoadSegmentsAreMigrated_ThenNotUniqueIsReported()
    {
        var roadNetwork = BuildV1RoadNetwork(otherGradeSeparatedJunction: CreateV1GradeSeparatedJunction(OtherJunctionId));

        var result = roadNetwork.Migrate(MigrateRoadNodesAndRoadSegments()
                .Add(new ModifyGradeSeparatedJunctionChange
                {
                    GradeSeparatedJunctionId = JunctionId,
                    LowerRoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    UpperRoadSegmentId = TestData.Segment2Added.RoadSegmentId,
                    Type = GradeSeparatedJunctionTypeV2.Brug
                })
                .Add(new ModifyGradeSeparatedJunctionChange
                {
                    GradeSeparatedJunctionId = OtherJunctionId,
                    LowerRoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    UpperRoadSegmentId = TestData.Segment2Added.RoadSegmentId,
                    Type = GradeSeparatedJunctionTypeV2.Tunnel
                }),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        result.Problems.Should().Contain(new GradeSeparatedJunctionNotUnique(JunctionId, OtherJunctionId));
    }

    private ScopedRoadNetwork BuildV1RoadNetwork(
        GradeSeparatedJunction? gradeSeparatedJunction = null,
        GradeSeparatedJunction? otherGradeSeparatedJunction = null)
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
                gradeSeparatedJunction ?? CreateV1GradeSeparatedJunction(JunctionId),
                ..otherGradeSeparatedJunction is not null ? new[] { otherGradeSeparatedJunction } : []
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

    private GradeSeparatedJunction CreateV1GradeSeparatedJunction(GradeSeparatedJunctionId gradeSeparatedJunctionId)
    {
        return GradeSeparatedJunction.CreateForMigration(
            gradeSeparatedJunctionId,
            TestData.Segment1Added.RoadSegmentId,
            TestData.Segment2Added.RoadSegmentId,
            new Point(5, 0) { SRID = WellknownSrids.Lambert08 }.ToJunctionGeometry());
    }

    private RoadSegment CreateV1RoadSegment(RoadSegmentId roadSegmentId, Point start, Point end, RoadNodeId startNodeId, RoadNodeId endNodeId)
    {
        return RoadSegment.CreateForMigration(
            roadSegmentId,
            BuildRoadSegmentGeometry(start, end),
            RoadSegmentStatusV2.Gerealiseerd,
            startNodeId,
            endNodeId);
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
