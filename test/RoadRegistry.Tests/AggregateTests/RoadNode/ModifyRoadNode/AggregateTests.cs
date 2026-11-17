namespace RoadRegistry.Tests.AggregateTests.RoadNode.ModifyRoadNode;

using AutoFixture;
using Extensions;
using FluentAssertions;
using Framework;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events.V2;
using RoadNode = RoadRegistry.RoadNode.RoadNode;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenRoadNodeModified()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var nodeAdded = Fixture.Create<RoadNodeWasAdded>();
        var node = RoadNode.Create(nodeAdded)
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadNodeChange>();

        // Act
        var problems = node.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        node.GetChanges().Should().HaveCount(1);

        var nodeModified = (RoadNodeWasChanged)node.GetChanges().Single();
        nodeModified.RoadNodeId.Should().Be(node.RoadNodeId);
        nodeModified.Geometry.Should().BeEquivalentTo(change.Geometry);
    }

    [Fact]
    public void WhenNoChangeDetected_ThenNone()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var nodeAdded = Fixture.Create<RoadNodeWasAdded>();
        var node = RoadNode.Create(nodeAdded)
            .WithoutChanges();
        var change = new ModifyRoadNodeChange
        {
            RoadNodeId = nodeAdded.RoadNodeId,
            Geometry = nodeAdded.Geometry,
            Grensknoop = nodeAdded.Grensknoop
        };

        // Act
        var problems = node.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        node.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var nodeAdded = Fixture.Create<RoadNodeWasAdded>();
        var node = RoadNode.Create(nodeAdded);
        var evt = Fixture.Create<RoadNodeWasChanged>();

        // Act
        node.Apply(evt);

        // Assert
        node.RoadNodeId.Should().Be(evt.RoadNodeId);
        node.Geometry.Should().Be(evt.Geometry);
    }

    // A removed road node is still loaded with the road network, so a caller can reach this. Nothing can be changed
    // about it any more.
    [Fact]
    public void GivenRemoved_ThenIsRemovedProblem()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var node = RoadNode.Create(Fixture.Create<RoadNodeWasAdded>());
        node.Remove(TestData.Provenance);
        node.WithoutChanges();

        var change = Fixture.Create<ModifyRoadNodeChange>();

        // Act
        var problems = node.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().Contain(x => x.Reason == "RoadNodeIsRemoved");
        node.GetChanges().Should().BeEmpty();
    }
}
