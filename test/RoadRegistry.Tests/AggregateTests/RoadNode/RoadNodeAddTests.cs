namespace RoadRegistry.Tests.AggregateTests.RoadNode;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.BackOffice;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events;

public class RoadNodeAddTests : RoadNetworkTestBase
{
    [Fact]
    public void ThenRoadNodeAdded()
    {
        // Arrange
        var change = Fixture.Create<AddRoadNodeChange>();

        // Act
        var (node, problems) = RoadNode.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.HasError().Should().BeFalse();
        node.GetChanges().Should().HaveCount(1);

        var nodeAdded = (RoadNodeAdded)node.GetChanges().Single();
        nodeAdded.RoadNodeId.Should().Be(new RoadNodeId(1));
        nodeAdded.Type.Should().Be(change.Type);
        nodeAdded.Geometry.Should().Be(change.Geometry.ToGeometryObject());
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var evt = Fixture.Create<RoadNodeAdded>();

        // Act
        var node = RoadNode.Create(evt);

        // Assert
        node.RoadNodeId.Should().Be(evt.RoadNodeId);
        node.Type.Should().Be(evt.Type);
        node.Geometry.Should().Be(evt.Geometry.AsPoint());
    }
}
