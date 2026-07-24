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

        var nodeModified = (GradeSeparatedJunctionWasModified)node.GetChanges().Single();
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
        var modified = (GradeSeparatedJunctionWasModified)node.GetChanges().Single();
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
        var evt = Fixture.Create<GradeSeparatedJunctionWasModified>();

        // Act
        node.Apply(evt);

        // Assert
        node.GradeSeparatedJunctionId.Should().Be(evt.GradeSeparatedJunctionId);
        node.LowerRoadSegmentId.Should().Be(evt.LowerRoadSegmentId!.Value);
        node.UpperRoadSegmentId.Should().Be(evt.UpperRoadSegmentId!.Value);
        node.Type.Should().Be(evt.Type);
    }
}
