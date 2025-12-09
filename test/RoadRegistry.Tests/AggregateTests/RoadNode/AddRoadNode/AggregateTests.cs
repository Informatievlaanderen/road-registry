namespace RoadRegistry.Tests.AggregateTests.RoadNode.AddRoadNode;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events;
using RoadRegistry.RoadNode.Events.V2;
using RoadNode = RoadRegistry.RoadNode.RoadNode;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenRoadNodeAdded()
    {
        // Arrange
        var change = Fixture.Create<AddRoadNodeChange>();

        // Act
        var (node, problems) = RoadNode.Add(change, new FakeProvenance(), new FakeRoadNetworkIdGenerator());

        // Assert
        problems.Should().HaveNoError();
        node.GetChanges().Should().HaveCount(1);

        var nodeAdded = (RoadNodeAdded)node.GetChanges().Single();
        nodeAdded.RoadNodeId.Should().Be(new RoadNodeId(1));
        nodeAdded.Type.Should().Be(change.Type);
        nodeAdded.Geometry.Should().Be(change.Geometry.ToGeometryObject());
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
        node.Geometry.Should().Be(evt.Geometry.ToPoint());
        node.Origin.Timestamp.Should().Be(evt.Provenance.Timestamp);
        node.Origin.OrganizationId.Should().Be(new OrganizationId(evt.Provenance.Operator));
        node.LastModified.Timestamp.Should().Be(evt.Provenance.Timestamp);
        node.LastModified.OrganizationId.Should().Be(new OrganizationId(evt.Provenance.Operator));
    }
}
