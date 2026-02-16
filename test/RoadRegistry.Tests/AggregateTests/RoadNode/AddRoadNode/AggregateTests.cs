namespace RoadRegistry.Tests.AggregateTests.RoadNode.AddRoadNode;

using AutoFixture;
using Extensions;
using FluentAssertions;
using Framework;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events;
using RoadRegistry.RoadNode.Events.V2;
using RoadNode = RoadRegistry.RoadNode.RoadNode;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenRoadNodeAdded()
    {
        // Arrange
        var change = Fixture.Create<AddRoadNodeChange>();

        // Act
        var (node, problems) = RoadNode.Add(change, new FakeProvenance(), new FakeRoadNetworkIdGenerator());

        // Assert
        problems.Should().HaveNoError();
        node.GetChanges().Should().HaveCount(1);

        var nodeAdded = (RoadNodeWasAdded)node.GetChanges().Single();
        nodeAdded.RoadNodeId.Should().Be(new RoadNodeId(1));
        nodeAdded.Geometry.Should().BeEquivalentTo(change.Geometry);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var evt = Fixture.Create<RoadNodeWasAdded>();

        // Act
        var node = RoadNode.Create(evt);

        // Assert
        node.RoadNodeId.Should().Be(evt.RoadNodeId);
        node.Geometry.Should().Be(evt.Geometry);
    }
}
