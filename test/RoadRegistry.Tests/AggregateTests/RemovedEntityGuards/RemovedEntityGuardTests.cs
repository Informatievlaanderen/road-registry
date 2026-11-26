namespace RoadRegistry.Tests.AggregateTests.RemovedEntityGuards;

using System.Linq;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
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
using RoadRegistry.ValueObjects;
using GradeJunction = RoadRegistry.GradeJunction.GradeJunction;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

// An entity that was removed is still loaded with the road network - the document stays, a removal only marks it - so
// every action that changes an existing entity can be handed one. None of them may record anything on it: the read
// models dropped their record at the removal and throw on a change they cannot find, which is what brought the
// grade separated junction projections to a halt on staging.
//
// Each aggregate's own test folder covers its Modify(); this covers the rest of the mutating methods in one place, so
// a newly added one that forgets the guard stands out.
public class RemovedEntityGuardTests : AggregateTestBase
{
    private RoadNode RemovedRoadNode()
    {
        var node = RoadNode.Create(Fixture.Create<RoadNodeWasAdded>());
        node.Remove(TestData.Provenance);
        return node.WithoutChanges();
    }

    private RoadSegment RemovedRoadSegment()
    {
        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>() with { Status = RoadSegmentStatusV2.Gerealiseerd });
        segment.Remove(TestData.Provenance);
        return segment.WithoutChanges();
    }

    private GradeSeparatedJunction RemovedGradeSeparatedJunction()
    {
        var junction = GradeSeparatedJunction.Create(Fixture.Create<GradeSeparatedJunctionWasAdded>());
        junction.RemoveBecauseOfMigration(TestData.Provenance);
        return junction.WithoutChanges();
    }

    private ScopedRoadNetworkChangeContext Context(params RoadNode[] roadNodes)
    {
        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(), roadNodes, [], []);
        return new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance);
    }

    [Fact]
    public void RoadSegmentAddEuropeanRoad()
    {
        var segment = RemovedRoadSegment();

        var problems = segment.AddEuropeanRoad(Fixture.Create<AddRoadSegmentToEuropeanRoadChange>(), TestData.Provenance);

        problems.Should().Contain(x => x.Reason == "RoadSegmentIsRemoved");
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadSegmentRemoveEuropeanRoad()
    {
        var segment = RemovedRoadSegment();

        var problems = segment.RemoveEuropeanRoad(Fixture.Create<RemoveRoadSegmentFromEuropeanRoadChange>(), TestData.Provenance);

        problems.Should().Contain(x => x.Reason == "RoadSegmentIsRemoved");
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadSegmentAddNationalRoad()
    {
        var segment = RemovedRoadSegment();

        var problems = segment.AddNationalRoad(Fixture.Create<AddRoadSegmentToNationalRoadChange>(), TestData.Provenance);

        problems.Should().Contain(x => x.Reason == "RoadSegmentIsRemoved");
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadSegmentRemoveNationalRoad()
    {
        var segment = RemovedRoadSegment();

        var problems = segment.RemoveNationalRoad(Fixture.Create<RemoveRoadSegmentFromNationalRoadChange>(), TestData.Provenance);

        problems.Should().Contain(x => x.Reason == "RoadSegmentIsRemoved");
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadSegmentHistorize()
    {
        var segment = RemovedRoadSegment();

        var problems = segment.Historize(Context());

        problems.Should().Contain(x => x.Reason == "RoadSegmentIsRemoved");
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadSegmentMerge()
    {
        var segment = RemovedRoadSegment();

        var problems = segment.Merge(Fixture.Create<MergeRoadSegmentChange>(), Context());

        problems.Should().Contain(x => x.Reason == "RoadSegmentIsRemoved");
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadSegmentMigrate()
    {
        var segment = RemovedRoadSegment();

        var problems = segment.Migrate(Fixture.Create<MigrateRoadSegmentChange>(), Context());

        problems.Should().Contain(x => x.Reason == "RoadSegmentIsRemoved");
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadSegmentRetireBecauseOfMerger()
    {
        var segment = RemovedRoadSegment();

        var problems = segment.RetireBecauseOfMerger(Fixture.Create<RoadSegmentId>(), TestData.Provenance);

        problems.Should().Contain(x => x.Reason == "RoadSegmentIsRemoved");
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadSegmentRetireBecauseOfSplit()
    {
        var segment = RemovedRoadSegment();

        var problems = segment.RetireBecauseOfSplit(TestData.Provenance);

        problems.Should().Contain(x => x.Reason == "RoadSegmentIsRemoved");
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadSegmentSplit()
    {
        var segment = RemovedRoadSegment();

        var problems = segment.Split([Fixture.Create<RoadSegmentId>(), Fixture.Create<RoadSegmentId>()], null, TestData.Provenance);

        problems.Should().Contain(x => x.Reason == "RoadSegmentIsRemoved");
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadSegmentChangeStatusToConnected()
    {
        var segment = RemovedRoadSegment();

        var problems = segment.ChangeStatusToConnected(
            RoadSegmentStatusPaths.Get(RoadSegmentStatusV2.Gepland, RoadSegmentStatusV2.Gerealiseerd).Single(),
            segment.Geometry,
            segment.Attributes!,
            Context());

        problems.Should().Contain(x => x.Reason == "RoadSegmentIsRemoved");
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadSegmentChangeStatusFromConnected()
    {
        var segment = RemovedRoadSegment();

        var problems = segment.ChangeStatusFromConnected(
            RoadSegmentStatusPaths.Get(RoadSegmentStatusV2.Gerealiseerd, RoadSegmentStatusV2.Gepland).Single(),
            Context());

        problems.Should().Contain(x => x.Reason == "RoadSegmentIsRemoved");
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadSegmentChangeStatusWhileUnconnected()
    {
        var segment = RemovedRoadSegment();

        var problems = segment.ChangeStatusWhileUnconnected(
            RoadSegmentStatusPaths.Get(RoadSegmentStatusV2.Gepland, RoadSegmentStatusV2.NietGerealiseerd).Single(),
            Context());

        problems.Should().Contain(x => x.Reason == "RoadSegmentIsRemoved");
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadNodeMigrate()
    {
        var node = RemovedRoadNode();

        var problems = node.Migrate(Fixture.Create<MigrateRoadNodeChange>() with { RoadNodeId = node.RoadNodeId }, TestData.Provenance);

        problems.Should().Contain(x => x.Reason == "RoadNodeIsRemoved");
        node.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadNodeVerifyTopologyAndUpdateType()
    {
        var node = RemovedRoadNode();

        var problems = node.VerifyTopologyAndUpdateType(
            new LazyQuadtree<RoadSegment>(_ => { }),
            new InMemoryRoadNetworkIdGenerator(initialValue: 100),
            Context(node),
            RoadSegmentMerging.NotAllowed);

        problems.Should().Contain(x => x.Reason == "RoadNodeIsRemoved");
        node.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void GradeSeparatedJunctionMigrate()
    {
        var junction = RemovedGradeSeparatedJunction();

        var problems = junction.Migrate(Fixture.Create<MigrateGradeSeparatedJunctionChange>(), TestData.Provenance);

        problems.Should().Contain(x => x.Reason == "GradeSeparatedJunctionIsRemoved");
        junction.GetChanges().Should().BeEmpty();
    }

    // Removing what is already removed is not an error - it is simply nothing to do, the same as Remove() has always
    // treated it. What must not happen is a second removal event.
    [Fact]
    public void RemoveBecauseOfMigrationIsIdempotent()
    {
        var node = RemovedRoadNode();
        var segment = RemovedRoadSegment();
        var junction = RemovedGradeSeparatedJunction();

        node.RemoveBecauseOfMigration(TestData.Provenance).Should().HaveNoError();
        segment.RemoveBecauseOfMigration(TestData.Provenance).Should().HaveNoError();
        junction.RemoveBecauseOfMigration(TestData.Provenance).Should().HaveNoError();

        node.GetChanges().Should().BeEmpty();
        segment.GetChanges().Should().BeEmpty();
        junction.GetChanges().Should().BeEmpty();
    }

    // The two mutators that return nothing cannot report a problem, so they say so the way the rest of this codebase
    // says 'this should not happen'.
    [Fact]
    public void RoadSegmentRelinkStreetNameThrows()
    {
        var segment = RemovedRoadSegment();

        var act = () => segment.RelinkStreetName(
            Fixture.Create<StreetNameLocalId>(),
            Fixture.Create<StreetNameLocalId>(),
            TestData.Provenance);

        act.Should().Throw<System.InvalidOperationException>();
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void RoadSegmentChangeGeometryDrawMethodThrows()
    {
        var segment = RemovedRoadSegment();
        var otherDrawMethod = segment.Attributes!.GeometryDrawMethod == RoadSegmentGeometryDrawMethodV2.Ingemeten
            ? RoadSegmentGeometryDrawMethodV2.Ingeschetst
            : RoadSegmentGeometryDrawMethodV2.Ingemeten;

        var act = () => segment.ChangeGeometryDrawMethod(otherDrawMethod, Context());

        act.Should().Throw<System.InvalidOperationException>();
        segment.GetChanges().Should().BeEmpty();
    }

    // The crossings recompute their geometry over whatever the road network holds, so a removed one is skipped rather
    // than reported: there is nothing wrong with asking, only with recording the answer.
    [Fact]
    public void JunctionGeometryRecomputeSkipsRemovedOnes()
    {
        var gradeSeparatedJunction = RemovedGradeSeparatedJunction();
        var gradeJunction = GradeJunction.Create(Fixture.Create<GradeJunctionWasAdded>());
        gradeJunction.Remove(TestData.Provenance);
        gradeJunction.WithoutChanges();

        gradeSeparatedJunction.ChangeGeometry(Fixture.Create<JunctionGeometry>(), TestData.Provenance);
        gradeSeparatedJunction.EnsureGeometryFollowsMigration(TestData.Provenance);
        gradeJunction.ChangeGeometry(Fixture.Create<JunctionGeometry>(), TestData.Provenance);

        gradeSeparatedJunction.GetChanges().Should().BeEmpty();
        gradeJunction.GetChanges().Should().BeEmpty();
    }
}
