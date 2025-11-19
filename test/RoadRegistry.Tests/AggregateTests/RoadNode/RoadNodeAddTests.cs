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

public class RoadNodeAddTests : RoadNetworkTestBase
{
    [Fact]
    public void ThenRoadNodeAdded()
    {
        // Arrange
        var change = Fixture.Create<AddRoadNodeChange>();

        // Act
        var (node, problems) = RoadNode.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.Should().HaveNoError();
        node.GetChanges().Should().HaveCount(1);

        var nodeAdded = (RoadNodeAdded)node.GetChanges().Single();
        nodeAdded.RoadNodeId.Should().Be(new RoadNodeId(1));
        nodeAdded.Type.Should().Be(change.Type);
        nodeAdded.Geometry.Should().Be(change.Geometry.ToGeometryObject());
    }

    [Fact]
    public Task WhenAddingMultipleNodesWithSameTemporaryId_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1StartNode)
            )
            .ThenContainsProblems(new Error("RoadNodeTemporaryIdNotUnique",
                new ProblemParameter("TemporaryId", TestData.AddSegment1StartNode.TemporaryId.ToString())
            ))
        );
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var evt = Fixture.Create<RoadNodeAdded>();

        // Act
        var node = RoadNode.Create(evt);

        // Assert
        node.RoadNodeId.Should().Be(evt.RoadNodeId);
        node.Type.Should().Be(evt.Type);
        node.Geometry.Should().Be(evt.Geometry.AsPoint());
    }
}
