namespace RoadRegistry.Tests.AggregateTests.RoadNode.RemoveRoadNodeBecauseOfMigration;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadNode = RoadRegistry.RoadNode.RoadNode;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenRoadNodeRemoved()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var node = RoadNode.Create(Fixture.Create<RoadNodeWasAdded>())
            .WithoutChanges();

        // Act
        var problems = node.RemoveBecauseOfMigration(Fixture.Create<Provenance>());

        // Assert
        problems.Should().HaveNoError();
        node.GetChanges().Should().HaveCount(1);

        var nodeRemoved = (RoadNodeWasRemovedBecauseOfMigration)node.GetChanges().Single();
        nodeRemoved.RoadNodeId.Should().Be(node.RoadNodeId);
        nodeRemoved.Geometry.Should().Be(node.Geometry);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var nodeAdded = Fixture.Create<RoadNodeWasAdded>();
        var node = RoadNode.Create(nodeAdded);

        var evt = Fixture.Create<RoadNodeWasRemovedBecauseOfMigration>();

        // Act
        node.Apply(evt);

        // Assert
        node.RoadNodeId.Should().Be(evt.RoadNodeId);
        node.IsRemoved.Should().BeTrue();
        node.Geometry.Should().Be(nodeAdded.Geometry);
    }
}
