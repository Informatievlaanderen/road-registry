namespace RoadRegistry.Tests.AggregateTests.RoadNode.RemoveRoadNode;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.BackOffice;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Events;
using RoadNode = RoadRegistry.RoadNode.RoadNode;

public class AggregateTests : AggregateTestBase
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
        problems.Should().HaveNoError();
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
        node.Geometry.Should().Be(nodeAdded.Geometry.ToPoint());
    }
}
