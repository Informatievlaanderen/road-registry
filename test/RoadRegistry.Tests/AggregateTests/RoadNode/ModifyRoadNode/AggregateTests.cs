namespace RoadRegistry.Tests.AggregateTests.RoadNode.ModifyRoadNode;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using FluentAssertions;
using Framework;
using RoadRegistry.BackOffice;
using RoadRegistry.RoadNode;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.RoadNode.Events;
using RoadNode = RoadRegistry.RoadNode.RoadNode;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenRoadNodeModified()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var nodeAdded = Fixture.Create<RoadNodeAdded>();
        var node = RoadNode.Create(nodeAdded)
            .WithoutChanges();
        var change = Fixture.Create<ModifyRoadNodeChange>();

        // Act
        var problems = node.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        node.GetChanges().Should().HaveCount(1);

        var nodeModified = (RoadNodeModified)node.GetChanges().Single();
        nodeModified.RoadNodeId.Should().Be(node.RoadNodeId);
        nodeModified.Type.Should().Be(change.Type);
        nodeModified.Geometry.Should().Be(change.Geometry!.ToGeometryObject());
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadNodeId>();

        var nodeAdded = Fixture.Create<RoadNodeAdded>();
        var node = RoadNode.Create(nodeAdded);
        var evt = Fixture.Create<RoadNodeModified>();

        // Act
        node.Apply(evt);

        // Assert
        node.RoadNodeId.Should().Be(evt.RoadNodeId);
        node.Type.Should().Be(evt.Type);
        node.Geometry.Should().Be(evt.Geometry!.ToPoint());
        node.Origin.Timestamp.Should().Be(nodeAdded.Provenance.Timestamp);
        node.Origin.OrganizationId.Should().Be(new OrganizationId(nodeAdded.Provenance.Operator));
        node.LastModified.Timestamp.Should().Be(evt.Provenance.Timestamp);
        node.LastModified.OrganizationId.Should().Be(new OrganizationId(evt.Provenance.Operator));
    }
}
