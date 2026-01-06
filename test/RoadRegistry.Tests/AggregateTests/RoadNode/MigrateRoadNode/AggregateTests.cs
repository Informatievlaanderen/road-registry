namespace RoadRegistry.Tests.AggregateTests.RoadNode.MigrateRoadNode;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadNode = RoadRegistry.RoadNode.RoadNode;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenRoadNodeMigrated()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var change = Fixture.Create<MigrateRoadNodeChange>();

        // Act
        var (node, problems) = RoadNode.Migrate(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        node.GetChanges().Should().HaveCount(1);

        var nodeModified = (RoadNodeWasMigrated)node.GetChanges().Single();
        nodeModified.RoadNodeId.Should().Be(node.RoadNodeId);
        nodeModified.Type.Should().Be(change.Type);
        nodeModified.Geometry.Should().Be(change.Geometry!.ToGeometryObject());
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var evt = Fixture.Create<RoadNodeWasMigrated>();

        // Act
        var node = RoadNode.Create(evt);

        // Assert
        node.RoadNodeId.Should().Be(evt.RoadNodeId);
        node.Type.Should().Be(evt.Type);
        node.Geometry.Should().Be(evt.Geometry!.ToGeometry());
    }
}
