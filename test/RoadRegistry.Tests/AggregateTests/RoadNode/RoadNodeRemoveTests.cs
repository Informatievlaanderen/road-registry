namespace RoadRegistry.Tests.AggregateTests.RoadNode;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.BackOffice;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Events;

public class RoadNodeRemoveTests : RoadNetworkTestBase
{
    [Fact]
    public void ThenRoadNodeRemoved()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var node = RoadNode.Create(Fixture.Create<RoadNodeAdded>())
            .WithoutChanges();

        // Act
        var problems = node.Remove();

        // Assert
        problems.HasError().Should().BeFalse();
        node.GetChanges().Should().HaveCount(1);

        var nodeRemoved = (RoadNodeRemoved)node.GetChanges().Single();
        nodeRemoved.RoadNodeId.Should().Be(node.RoadNodeId);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var nodeAdded = Fixture.Create<RoadNodeAdded>();
        var node = RoadNode.Create(nodeAdded);

        var evt = Fixture.Create<RoadNodeRemoved>();

        // Act
        node.Apply(evt);

        // Assert
        node.RoadNodeId.Should().Be(evt.RoadNodeId);
        node.IsRemoved.Should().BeTrue();
        node.Type.Should().Be(nodeAdded.Type);
        node.Geometry.Should().Be(nodeAdded.Geometry.AsPoint());
    }
}
