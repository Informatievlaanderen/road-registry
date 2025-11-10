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

        var roadNode = RoadNode.Create(Fixture.Create<RoadNodeAdded>());

        // Act
        var problems = roadNode.Remove();

        // Assert
        problems.HasError().Should().BeFalse();
        roadNode.GetChanges().Should().HaveCount(2);

        var roadNodeModified = (RoadNodeRemoved)roadNode.GetChanges().Last();
        roadNodeModified.RoadNodeId.Should().Be(roadNode.RoadNodeId);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var roadNodeAdded = Fixture.Create<RoadNodeAdded>();
        var roadNode = RoadNode.Create(roadNodeAdded);

        var evt = Fixture.Create<RoadNodeRemoved>();

        // Act
        roadNode.Apply(evt);

        // Assert
        roadNode.RoadNodeId.Should().Be(evt.RoadNodeId);
        roadNode.Type.Should().Be(roadNodeAdded.Type);
        roadNode.Geometry.Should().Be(roadNodeAdded.Geometry.AsPoint());
        roadNode.IsRemoved.Should().BeTrue();
    }
}
