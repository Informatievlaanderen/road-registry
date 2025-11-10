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
        var (roadNode, problems) = RoadNode.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.HasError().Should().BeFalse();
        roadNode.GetChanges().Should().HaveCount(1);

        var roadNodeModified = (RoadNodeAdded)roadNode.GetChanges().Single();
        roadNodeModified.RoadNodeId.Should().Be(new RoadNodeId(1));
        roadNodeModified.Type.Should().Be(change.Type);
        roadNodeModified.Geometry.Should().Be(change.Geometry.ToGeometryObject());
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var evt = Fixture.Create<RoadNodeAdded>();

        // Act
        var roadNode = RoadNode.Create(evt);

        // Assert
        roadNode.RoadNodeId.Should().Be(evt.RoadNodeId);
        roadNode.Type.Should().Be(evt.Type);
        roadNode.Geometry.Should().Be(evt.Geometry.AsPoint());
    }
}
