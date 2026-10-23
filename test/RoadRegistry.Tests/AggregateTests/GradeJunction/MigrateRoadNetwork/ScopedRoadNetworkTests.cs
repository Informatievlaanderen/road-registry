namespace RoadRegistry.Tests.AggregateTests.GradeJunction.MigrateRoadNetwork;

using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using GradeJunction = RoadRegistry.GradeJunction.GradeJunction;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

// How the migration treats a crossing of which it migrates only part. The check that decides between a gelijkgrondse
// and an ongelijkgrondse kruising reads the traffic attributes of both segments, and a segment that has not migrated
// yet has none - so a crossing is only judged once both its segments are V2, and never when an ongelijkgrondse
// kruising already records it.
public class ScopedRoadNetworkTests : RoadNetworkTestBase
{
    private static readonly GradeSeparatedJunctionId GradeSeparatedJunctionId = new(1);
    private static readonly GradeJunctionId GradeJunctionId = new(1);

    // Segment 1 runs west to east, segment 2 runs north to south; they cross at (5, 0), away from every road node.
    private readonly Point _segment1Start = new(0, 0);
    private readonly Point _segment1End = new(10, 0);
    private readonly Point _segment2Start = new(5, 5);
    private readonly Point _segment2End = new(5, -5);
    private readonly Point _crossing = new(5, 0) { SRID = WellknownSrids.Lambert08 };

    // Two crossing V1 segments carrying a V1 ongelijkgrondse kruising, with an inwinning that contains only one of the
    // two segments and a modify of the kruising. The kruising migrates, and nothing is recorded for the crossing
    // itself: it is already described by an ongelijkgrondse kruising, and the other segment is still V1 anyway.
    [Fact]
    public void GivenTwoCrossingV1SegmentsWithGradeSeparatedJunction_WhenOnlyOneSegmentIsMigrated_ThenNoGradeJunctionIsAdded()
    {
        var segment1Geometry = BuildRoadSegmentGeometry(_segment1Start, _segment1End);
        var segment2Geometry = BuildRoadSegmentGeometry(_segment2Start, _segment2End);

        var roadNetwork = BuildV1RoadNetwork(segment1Geometry, segment2Geometry,
            gradeSeparatedJunctions:
            [
                GradeSeparatedJunction.CreateForMigration(GradeSeparatedJunctionId, TestData.Segment1Added.RoadSegmentId, TestData.Segment2Added.RoadSegmentId, _crossing.ToJunctionGeometry())
            ]);

        var result = roadNetwork.Migrate(MigrateSegment1(segment1Geometry)
                .Add(new ModifyGradeSeparatedJunctionChange
                {
                    GradeSeparatedJunctionId = GradeSeparatedJunctionId,
                    LowerRoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    UpperRoadSegmentId = TestData.Segment2Added.RoadSegmentId,
                    Type = GradeSeparatedJunctionTypeV2.Brug
                }),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        result.Problems.Should().HaveNoError();

        CollectEvents(roadNetwork).Select(x => x.GetType()).Should().Equal(
            // the two road nodes of the migrated segment
            typeof(RoadNodeWasMigrated), typeof(RoadNodeTypeWasChanged),
            typeof(RoadNodeWasMigrated), typeof(RoadNodeTypeWasChanged),
            // the one segment the inwinning contains
            typeof(RoadSegmentWasMigrated),
            // and the ongelijkgrondse kruising, whose geometry follows the migration
            typeof(GradeSeparatedJunctionWasMigrated), typeof(GradeSeparatedJunctionGeometryWasChanged));

        // Only the segment in the inwinning becomes V2; the crossing segment stays V1.
        roadNetwork.RoadSegments[TestData.Segment1Added.RoadSegmentId].HasMigrated().Should().BeTrue();
        roadNetwork.RoadSegments[TestData.Segment2Added.RoadSegmentId].HasMigrated().Should().BeFalse();

        // The ongelijkgrondse kruising keeps its identifier and becomes V2.
        var gradeSeparatedJunction = roadNetwork.GradeSeparatedJunctions[GradeSeparatedJunctionId];
        gradeSeparatedJunction.HasMigrated().Should().BeTrue();
        gradeSeparatedJunction.IsRemoved.Should().BeFalse();
        result.Summary.GradeSeparatedJunctions.Modified.Should().Equal(GradeSeparatedJunctionId);
        result.Summary.GradeSeparatedJunctions.Removed.Should().BeEmpty();

        // No gelijkgrondse kruising next to it.
        roadNetwork.GradeJunctions.Should().BeEmpty();
        result.Summary.GradeJunctions.Added.Should().BeEmpty();
        result.Summary.GradeJunctions.Removed.Should().BeEmpty();
    }

    // The same crossing without any kruising - in V1 a crossing of a road and a cycle path simply has none - and an
    // inwinning that contains one segment at a time. The crossing is judged only once the second segment is in, when
    // both segments' traffic attributes are known.
    [Fact]
    public void GivenTwoCrossingV1SegmentsWithoutJunction_WhenTheSegmentsAreMigratedOneAtATime_ThenTheGradeJunctionFollowsTheSecondMigration()
    {
        var segment1Geometry = BuildRoadSegmentGeometry(_segment1Start, _segment1End);
        var segment2Geometry = BuildRoadSegmentGeometry(_segment2Start, _segment2End);

        var roadNetwork = BuildV1RoadNetwork(segment1Geometry, segment2Geometry);

        // The first inwinning carries the road.
        var result = roadNetwork.Migrate(MigrateSegment1(segment1Geometry),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        result.Problems.Should().HaveNoError();
        CollectEvents(roadNetwork).Select(x => x.GetType()).Should().Equal(
            typeof(RoadNodeWasMigrated), typeof(RoadNodeTypeWasChanged),
            typeof(RoadNodeWasMigrated), typeof(RoadNodeTypeWasChanged),
            typeof(RoadSegmentWasMigrated));
        roadNetwork.GradeJunctions.Should().BeEmpty();
        result.Summary.GradeJunctions.Added.Should().BeEmpty();

        // The second one carries the cycle path, which shares no traffic type with the road.
        roadNetwork.WithoutChanges();
        result = roadNetwork.Migrate(RoadNetworkChanges.Start()
                .WithProvenance(new FakeProvenance())
                .Add(MigrateRoadNode(TestData.Segment2StartNodeAdded.RoadNodeId, _segment2Start))
                .Add(MigrateRoadNode(TestData.Segment2EndNodeAdded.RoadNodeId, _segment2End))
                .Add(MigrateRoadSegment(TestData.AddSegment2, TestData.Segment2Added.RoadSegmentId, segment2Geometry, RoadSegmentTrafficDirection.None, RoadSegmentTrafficDirection.Both)),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        result.Problems.Should().HaveNoError();
        CollectEvents(roadNetwork).Select(x => x.GetType()).Should().Equal(
            typeof(RoadNodeWasMigrated), typeof(RoadNodeTypeWasChanged),
            typeof(RoadNodeWasMigrated), typeof(RoadNodeTypeWasChanged),
            typeof(RoadSegmentWasMigrated),
            typeof(GradeJunctionWasAdded));

        result.Summary.GradeJunctions.Added.Should().HaveCount(1);
        var gradeJunctionWasAdded = CollectEvents(roadNetwork).OfType<GradeJunctionWasAdded>().Should().ContainSingle().Which;
        new[] { gradeJunctionWasAdded.RoadSegmentId1, gradeJunctionWasAdded.RoadSegmentId2 }.Should()
            .BeEquivalentTo([TestData.Segment1Added.RoadSegmentId, TestData.Segment2Added.RoadSegmentId]);
        gradeJunctionWasAdded.Geometry.Should().Be(_crossing.ToJunctionGeometry());
    }

    // Both segments in one inwinning, sharing no traffic type, with an ongelijkgrondse kruising already recorded for
    // the crossing. The absent traffic overlap is not a reason to record a gelijkgrondse kruising next to it.
    [Fact]
    public void GivenTwoCrossingV1SegmentsWithGradeSeparatedJunction_WhenBothAreMigratedWithoutTrafficOverlap_ThenNoGradeJunctionIsAdded()
    {
        var segment1Geometry = BuildRoadSegmentGeometry(_segment1Start, _segment1End);
        var segment2Geometry = BuildRoadSegmentGeometry(_segment2Start, _segment2End);

        var roadNetwork = BuildV1RoadNetwork(segment1Geometry, segment2Geometry,
            gradeSeparatedJunctions:
            [
                GradeSeparatedJunction.CreateForMigration(GradeSeparatedJunctionId, TestData.Segment1Added.RoadSegmentId, TestData.Segment2Added.RoadSegmentId, _crossing.ToJunctionGeometry())
            ]);

        var result = roadNetwork.Migrate(MigrateSegment1(segment1Geometry)
                .Add(MigrateRoadNode(TestData.Segment2StartNodeAdded.RoadNodeId, _segment2Start))
                .Add(MigrateRoadNode(TestData.Segment2EndNodeAdded.RoadNodeId, _segment2End))
                .Add(MigrateRoadSegment(TestData.AddSegment2, TestData.Segment2Added.RoadSegmentId, segment2Geometry, RoadSegmentTrafficDirection.None, RoadSegmentTrafficDirection.Both))
                .Add(new ModifyGradeSeparatedJunctionChange
                {
                    GradeSeparatedJunctionId = GradeSeparatedJunctionId,
                    LowerRoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    UpperRoadSegmentId = TestData.Segment2Added.RoadSegmentId,
                    Type = GradeSeparatedJunctionTypeV2.Brug
                }),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        result.Problems.Should().HaveNoError();

        roadNetwork.RoadSegments[TestData.Segment1Added.RoadSegmentId].HasMigrated().Should().BeTrue();
        roadNetwork.RoadSegments[TestData.Segment2Added.RoadSegmentId].HasMigrated().Should().BeTrue();
        roadNetwork.GradeSeparatedJunctions[GradeSeparatedJunctionId].HasMigrated().Should().BeTrue();

        roadNetwork.GradeJunctions.Should().BeEmpty();
        result.Summary.GradeJunctions.Added.Should().BeEmpty();
        CollectEvents(roadNetwork).OfType<GradeJunctionWasAdded>().Should().BeEmpty();
    }

    // The state the old behaviour left behind: both kinds of kruising on one segment pair, one segment already V2 and
    // the other still V1. Migrating the remaining V1 segment gives it traffic attributes, both segments turn out to
    // carry car traffic, and the gelijkgrondse kruising is removed - a crossing where traffic types overlap cannot be
    // at grade, and the ongelijkgrondse kruising that records it is left untouched.
    [Fact]
    public void GivenACrossingWithBothKindsOfJunction_WhenTheV1SegmentIsMigrated_ThenTheGradeJunctionIsRemoved()
    {
        var segment1Geometry = BuildRoadSegmentGeometry(_segment1Start, _segment1End);
        var segment2Geometry = BuildRoadSegmentGeometry(_segment2Start, _segment2End);

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            roadNodes:
            [
                RoadNode.CreateForMigration(TestData.Segment1StartNodeAdded.RoadNodeId, _segment1Start.ToRoadNodeGeometry()),
                RoadNode.CreateForMigration(TestData.Segment1EndNodeAdded.RoadNodeId, _segment1End.ToRoadNodeGeometry()),
                CreateV2RoadNode(TestData.Segment2StartNodeAdded, _segment2Start),
                CreateV2RoadNode(TestData.Segment2EndNodeAdded, _segment2End)
            ],
            roadSegments:
            [
                RoadSegment.CreateForMigration(TestData.Segment1Added.RoadSegmentId, segment1Geometry, RoadSegmentStatusV2.Gerealiseerd, TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.RoadNodeId),
                CreateV2RoadSegment(TestData.Segment2Added, segment2Geometry, RoadSegmentTrafficDirection.Both)
            ],
            gradeSeparatedJunctions:
            [
                GradeSeparatedJunction.Create(new GradeSeparatedJunctionWasAdded
                {
                    GradeSeparatedJunctionId = GradeSeparatedJunctionId,
                    LowerRoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    UpperRoadSegmentId = TestData.Segment2Added.RoadSegmentId,
                    Type = GradeSeparatedJunctionTypeV2.Brug,
                    Geometry = _crossing.ToJunctionGeometry(),
                    Provenance = new(TestData.Provenance)
                }).WithoutChanges()
            ],
            gradeJunctions:
            [
                CreateV2GradeJunction()
            ]);

        var result = roadNetwork.Migrate(MigrateSegment1(segment1Geometry),
            TestData.Fixture.Create<DownloadId>(),
            new InMemoryRoadNetworkIdGenerator());

        result.Problems.Should().HaveNoError();

        CollectEvents(roadNetwork).Select(x => x.GetType()).Should().Equal(
            typeof(RoadNodeWasMigrated), typeof(RoadNodeTypeWasChanged),
            typeof(RoadNodeWasMigrated), typeof(RoadNodeTypeWasChanged),
            typeof(RoadSegmentWasMigrated),
            typeof(GradeJunctionWasRemoved));

        roadNetwork.RoadSegments[TestData.Segment1Added.RoadSegmentId].HasMigrated().Should().BeTrue();

        // The gelijkgrondse kruising is gone.
        result.Summary.GradeJunctions.Removed.Should().Equal(GradeJunctionId);
        result.Summary.GradeJunctions.Added.Should().BeEmpty();
        roadNetwork.GradeJunctions[GradeJunctionId].IsRemoved.Should().BeTrue();
        CollectEvents(roadNetwork).OfType<GradeJunctionWasRemoved>().Should().ContainSingle()
            .Which.GradeJunctionId.Should().Be(GradeJunctionId);

        // The ongelijkgrondse kruising is already V2 and its crossing did not move, so it records nothing.
        roadNetwork.GradeSeparatedJunctions[GradeSeparatedJunctionId].GetChanges().Should().BeEmpty();
        result.Summary.GradeSeparatedJunctions.Removed.Should().BeEmpty();
    }

    private ScopedRoadNetwork BuildV1RoadNetwork(
        RoadSegmentGeometry segment1Geometry,
        RoadSegmentGeometry segment2Geometry,
        IReadOnlyCollection<GradeSeparatedJunction>? gradeSeparatedJunctions = null)
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
                RoadSegment.CreateForMigration(TestData.Segment1Added.RoadSegmentId, segment1Geometry, RoadSegmentStatusV2.Gerealiseerd, TestData.Segment1StartNodeAdded.RoadNodeId, TestData.Segment1EndNodeAdded.RoadNodeId),
                RoadSegment.CreateForMigration(TestData.Segment2Added.RoadSegmentId, segment2Geometry, RoadSegmentStatusV2.Gerealiseerd, TestData.Segment2StartNodeAdded.RoadNodeId, TestData.Segment2EndNodeAdded.RoadNodeId)
            ],
            gradeSeparatedJunctions: gradeSeparatedJunctions);
    }

    // An inwinning carrying segment 1 - a road, open to car traffic - and its two road nodes.
    private RoadNetworkChanges MigrateSegment1(RoadSegmentGeometry segment1Geometry)
    {
        return RoadNetworkChanges.Start()
            .WithProvenance(new FakeProvenance())
            .Add(MigrateRoadNode(TestData.Segment1StartNodeAdded.RoadNodeId, _segment1Start))
            .Add(MigrateRoadNode(TestData.Segment1EndNodeAdded.RoadNodeId, _segment1End))
            .Add(MigrateRoadSegment(TestData.AddSegment1, TestData.Segment1Added.RoadSegmentId, segment1Geometry, RoadSegmentTrafficDirection.Both));
    }

    private static IReadOnlyCollection<object> CollectEvents(ScopedRoadNetwork roadNetwork)
    {
        return Enumerable.Empty<object>()
            .Concat(roadNetwork.RoadNodes.SelectMany(x => x.Value.GetChanges()))
            .Concat(roadNetwork.RoadSegments.SelectMany(x => x.Value.GetChanges()))
            .Concat(roadNetwork.GradeSeparatedJunctions.SelectMany(x => x.Value.GetChanges()))
            .Concat(roadNetwork.GradeJunctions.SelectMany(x => x.Value.GetChanges()))
            .ToArray();
    }

    private GradeJunction CreateV2GradeJunction()
    {
        return GradeJunction.Create(new GradeJunctionWasAdded
        {
            GradeJunctionId = GradeJunctionId,
            RoadSegmentId1 = TestData.Segment1Added.RoadSegmentId,
            RoadSegmentId2 = TestData.Segment2Added.RoadSegmentId,
            Geometry = _crossing.ToJunctionGeometry(),
            Provenance = new(TestData.Provenance)
        }).WithoutChanges();
    }

    private static RoadNode CreateV2RoadNode(RoadNodeWasAdded template, Point geometry)
    {
        return RoadNode.Create(template with
        {
            Geometry = geometry.ToRoadNodeGeometry(),
            Type = RoadNodeTypeV2.Eindknoop
        }).WithoutChanges();
    }

    private static RoadSegment CreateV2RoadSegment(RoadSegmentWasAdded template, RoadSegmentGeometry geometry, RoadSegmentTrafficDirection carTrafficDirection)
    {
        return RoadSegment.Create(template with
        {
            Geometry = geometry,
            Status = RoadSegmentStatusV2.Gerealiseerd,
            AccessRestriction = template.AccessRestriction.ForEntireGeometry(geometry),
            Category = template.Category.ForEntireGeometry(geometry),
            Morphology = template.Morphology.ForEntireGeometry(geometry),
            StreetNameId = template.StreetNameId.ForEntireGeometry(geometry),
            MaintenanceAuthorityId = template.MaintenanceAuthorityId.ForEntireGeometry(geometry),
            SurfaceType = template.SurfaceType.ForEntireGeometry(geometry),
            CarTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>(carTrafficDirection, geometry),
            BikeTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>(RoadSegmentTrafficDirection.None, geometry),
            PedestrianTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>(RoadSegmentPedestrianTrafficDirection.None, geometry)
        }).WithoutChanges();
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

    private static ModifyRoadSegmentChange MigrateRoadSegment(
        AddRoadSegmentChange attributes,
        RoadSegmentId roadSegmentId,
        RoadSegmentGeometry geometry,
        RoadSegmentTrafficDirection carTrafficDirection,
        RoadSegmentTrafficDirection? bikeTrafficDirection = null)
    {
        return new ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(roadSegmentId),
            Geometry = geometry,
            GeometryDrawMethod = attributes.GeometryDrawMethod,
            AccessRestriction = attributes.AccessRestriction,
            Category = attributes.Category,
            Morphology = attributes.Morphology,
            Status = RoadSegmentStatusV2.Gerealiseerd,
            StreetNameId = attributes.StreetNameId,
            MaintenanceAuthorityId = attributes.MaintenanceAuthorityId,
            SurfaceType = attributes.SurfaceType,
            CarTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>(carTrafficDirection, geometry),
            BikeTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>(bikeTrafficDirection ?? RoadSegmentTrafficDirection.None, geometry),
            PedestrianTrafficDirection = new RoadSegmentDynamicAttributeValues<RoadSegmentPedestrianTrafficDirection>(RoadSegmentPedestrianTrafficDirection.None, geometry)
        }.WithDynamicAttributePositionsOnEntireGeometryLength();
    }
}
