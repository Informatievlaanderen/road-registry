namespace RoadRegistry.Tests.AggregateTests.RoadNode;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events;
using RoadNode = RoadRegistry.RoadNode.RoadNode;

public class RoadNodeRemoveTests : RoadNetworkTestBase
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
    public Task GivenNoRoadNode_WhenRoadNetworkChange_ThenNotFound()
    {
        var change = Fixture.Create<RemoveRoadNodeChange>();

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(change)
            )
            .ThenProblems(new Error("RoadNodeNotFound", new ProblemParameter("NodeId", change.RoadNodeId.ToString())))
        );
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
        node.Geometry.Should().Be(nodeAdded.Geometry.AsPoint());
    }
}
