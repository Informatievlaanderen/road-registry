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

        var nodeAdded = Fixture.Create<RoadNodeWasAdded>();
        var node = RoadNode.Create(nodeAdded)
            .WithoutChanges();
        var change = Fixture.Create<MigrateRoadNodeChange>();

        // Act
        var problems = node.Migrate(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        node.GetChanges().Should().HaveCount(1);

        var nodeMigrated = (RoadNodeWasMigrated)node.GetChanges().Single();
        nodeMigrated.RoadNodeId.Should().Be(node.RoadNodeId);
        nodeMigrated.Geometry.Should().BeEquivalentTo(change.Geometry);
        nodeMigrated.Grensknoop.Should().Be(change.Grensknoop);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var nodeAdded = Fixture.Create<RoadNodeWasAdded>();
        var node = RoadNode.Create(nodeAdded)
            .WithoutChanges();
        var evt = Fixture.Create<RoadNodeWasMigrated>();

        // Act
        node.Apply(evt);

        // Assert
        node.RoadNodeId.Should().Be(evt.RoadNodeId);
        node.Geometry.Should().Be(evt.Geometry);
        node.Grensknoop.Should().Be(evt.Grensknoop);
    }
}
