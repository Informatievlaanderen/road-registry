namespace RoadRegistry.Tests.AggregateTests.RoadNode.ModifyRoadNode;

using AutoFixture;
using Extensions;
using FluentAssertions;
using Framework;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events.V2;
using RoadNode = RoadRegistry.RoadNode.RoadNode;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenRoadNodeModified()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var nodeAdded = Fixture.Create<RoadNodeWasAdded>();
        var node = RoadNode.Create(nodeAdded)
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadNodeChange>();

        // Act
        var problems = node.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        node.GetChanges().Should().HaveCount(1);

        var nodeModified = (RoadNodeWasModified)node.GetChanges().Single();
        nodeModified.RoadNodeId.Should().Be(node.RoadNodeId);
        nodeModified.Geometry.Should().BeEquivalentTo(change.Geometry);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var nodeAdded = Fixture.Create<RoadNodeWasAdded>();
        var node = RoadNode.Create(nodeAdded);
        var evt = Fixture.Create<RoadNodeWasModified>();

        // Act
        node.Apply(evt);

        // Assert
        node.RoadNodeId.Should().Be(evt.RoadNodeId);
        node.Geometry.Should().Be(evt.Geometry);
    }
}
