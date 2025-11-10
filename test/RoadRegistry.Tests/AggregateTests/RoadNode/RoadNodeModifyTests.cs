namespace RoadRegistry.Tests.AggregateTests.RoadNode;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.BackOffice;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events;

public class RoadNodeModifyTests : RoadNetworkTestBase
{
    [Fact]
    public void ThenRoadNodeModified()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var roadNode = RoadNode.Create(Fixture.Create<RoadNodeAdded>());
        var change = Fixture.Create<ModifyRoadNodeChange>();

        // Act
        var problems = roadNode.Modify(change);

        // Assert
        problems.HasError().Should().BeFalse();
        roadNode.GetChanges().Should().HaveCount(2);

        var roadNodeModified = (RoadNodeModified)roadNode.GetChanges().Last();
        roadNodeModified.RoadNodeId.Should().Be(roadNode.RoadNodeId);
        roadNodeModified.Type.Should().Be(change.Type);
        roadNodeModified.Geometry.Should().Be(change.Geometry!.ToGeometryObject());
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var roadNode = RoadNode.Create(Fixture.Create<RoadNodeAdded>());
        var evt = Fixture.Create<RoadNodeModified>();

        // Act
        roadNode.Apply(evt);

        // Assert
        roadNode.RoadNodeId.Should().Be(evt.RoadNodeId);
        roadNode.Type.Should().Be(evt.Type);
        roadNode.Geometry.Should().Be(evt.Geometry.AsPoint());
    }
}
