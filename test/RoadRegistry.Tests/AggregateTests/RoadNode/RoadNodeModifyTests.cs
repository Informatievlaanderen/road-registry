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

public class RoadNodeModifyTests : RoadNetworkTestBase
{
    [Fact]
    public void ThenRoadNodeModified()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var node = RoadNode.Create(Fixture.Create<RoadNodeAdded>())
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadNodeChange>();

        // Act
        var problems = node.Modify(change);

        // Assert
        problems.Should().HaveNoError();
        node.GetChanges().Should().HaveCount(1);

        var nodeModified = (RoadNodeModified)node.GetChanges().Single();
        nodeModified.RoadNodeId.Should().Be(node.RoadNodeId);
        nodeModified.Type.Should().Be(change.Type);
        nodeModified.Geometry.Should().Be(change.Geometry!.ToGeometryObject());
    }

    [Fact]
    public Task GivenNoRoadNode_WhenRoadNetworkChange_ThenNotFound()
    {
        var change = Fixture.Create<ModifyRoadNodeChange>();

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

        var node = RoadNode.Create(Fixture.Create<RoadNodeAdded>());
        var evt = Fixture.Create<RoadNodeModified>();

        // Act
        node.Apply(evt);

        // Assert
        node.RoadNodeId.Should().Be(evt.RoadNodeId);
        node.Type.Should().Be(evt.Type);
        node.Geometry.Should().Be(evt.Geometry.AsPoint());
    }
}
