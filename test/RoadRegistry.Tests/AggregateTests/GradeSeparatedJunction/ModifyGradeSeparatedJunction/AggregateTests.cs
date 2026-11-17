namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.ModifyGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenGradeSeparatedJunctionModified()
    {
        // Arrange
        Fixture.Freeze<GradeSeparatedJunctionId>();

        var nodeAdded = Fixture.Create<GradeSeparatedJunctionWasAdded>();
        var node = GradeSeparatedJunction.Create(nodeAdded)
            .WithoutChanges();
        var change = Fixture.Create<ModifyGradeSeparatedJunctionChange>();

        // Act
        var problems = node.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        node.GetChanges().Should().HaveCount(1);

        var nodeModified = (GradeSeparatedJunctionWasChanged)node.GetChanges().Single();
        nodeModified.GradeSeparatedJunctionId.Should().Be(node.GradeSeparatedJunctionId);
        nodeModified.LowerRoadSegmentId.Should().Be(change.LowerRoadSegmentId);
        nodeModified.UpperRoadSegmentId.Should().Be(change.UpperRoadSegmentId);
        nodeModified.Type.Should().Be(change.Type);
    }

    [Fact]
    public void GivenNothingToModify_ThenNoRoadSegmentSpecifiedProblem()
    {
        // Arrange
        Fixture.Freeze<GradeSeparatedJunctionId>();

        var nodeAdded = Fixture.Create<GradeSeparatedJunctionWasAdded>();
        var node = GradeSeparatedJunction.Create(nodeAdded)
            .WithoutChanges();
        // Neither road segment nor the type is set, so there is nothing to change.
        var change = new ModifyGradeSeparatedJunctionChange
        {
            GradeSeparatedJunctionId = node.GradeSeparatedJunctionId
        };

        // Act
        var problems = node.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().Contain(x => x.Reason == "GradeSeparatedJunctionNoRoadSegmentSpecified");
        node.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void GivenOnlyType_ThenGradeSeparatedJunctionModified()
    {
        // Arrange
        Fixture.Freeze<GradeSeparatedJunctionId>();

        var nodeAdded = Fixture.Create<GradeSeparatedJunctionWasAdded>();
        var node = GradeSeparatedJunction.Create(nodeAdded)
            .WithoutChanges();
        // A type-only modification (no road segments) is allowed.
        var change = new ModifyGradeSeparatedJunctionChange
        {
            GradeSeparatedJunctionId = node.GradeSeparatedJunctionId,
            Type = GradeSeparatedJunctionTypeV2.Tunnel
        };

        // Act
        var problems = node.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        var modified = (GradeSeparatedJunctionWasChanged)node.GetChanges().Single();
        modified.Type.Should().Be(GradeSeparatedJunctionTypeV2.Tunnel);
        modified.LowerRoadSegmentId.Should().BeNull();
        modified.UpperRoadSegmentId.Should().BeNull();
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<GradeSeparatedJunctionId>();

        var nodeAdded = Fixture.Create<GradeSeparatedJunctionWasAdded>();
        var node = GradeSeparatedJunction.Create(nodeAdded);
        var evt = Fixture.Create<GradeSeparatedJunctionWasChanged>();

        // Act
        node.Apply(evt);

        // Assert
        node.GradeSeparatedJunctionId.Should().Be(evt.GradeSeparatedJunctionId);
        node.LowerRoadSegmentId.Should().Be(evt.LowerRoadSegmentId!.Value);
        node.UpperRoadSegmentId.Should().Be(evt.UpperRoadSegmentId!.Value);
        node.Type.Should().Be(evt.Type);
    }

    // A removed crossing is still loaded with the road network, so a caller can reach this - as the merge of two road
    // segments did. Nothing can be changed about it any more.
    [Fact]
    public void GivenRemoved_ThenIsRemovedProblem()
    {
        // Arrange
        Fixture.Freeze<GradeSeparatedJunctionId>();

        var junction = GradeSeparatedJunction.Create(Fixture.Create<GradeSeparatedJunctionWasAdded>());
        junction.Remove(TestData.Provenance);
        junction.WithoutChanges();

        var change = Fixture.Create<ModifyGradeSeparatedJunctionChange>();

        // Act
        var problems = junction.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().Contain(x => x.Reason == "GradeSeparatedJunctionIsRemoved");
        junction.GetChanges().Should().BeEmpty();
    }

    // The same for a crossing a migration removed: that is how the one on staging came to be changed.
    [Fact]
    public void GivenRemovedBecauseOfMigration_ThenIsRemovedProblem()
    {
        // Arrange
        Fixture.Freeze<GradeSeparatedJunctionId>();

        var junction = GradeSeparatedJunction.Create(Fixture.Create<GradeSeparatedJunctionWasAdded>());
        junction.RemoveBecauseOfMigration(TestData.Provenance);
        junction.WithoutChanges();

        var change = Fixture.Create<ModifyGradeSeparatedJunctionChange>();

        // Act
        var problems = junction.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().Contain(x => x.Reason == "GradeSeparatedJunctionIsRemoved");
        junction.GetChanges().Should().BeEmpty();
    }
}
