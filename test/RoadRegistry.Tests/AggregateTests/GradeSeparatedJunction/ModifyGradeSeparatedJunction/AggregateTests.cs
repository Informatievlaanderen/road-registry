namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.ModifyGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.GradeSeparatedJunction.Events;
using GradeSeparatedJunction = RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenGradeSeparatedJunctionModified()
    {
        // Arrange
        Fixture.Freeze<GradeSeparatedJunctionId>();

        var nodeAdded = Fixture.Create<GradeSeparatedJunctionAdded>();
        var node = GradeSeparatedJunction.Create(nodeAdded)
            .WithoutChanges();
        var change = Fixture.Create<ModifyGradeSeparatedJunctionChange>();

        // Act
        var problems = node.Modify(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        node.GetChanges().Should().HaveCount(1);

        var nodeModified = (GradeSeparatedJunctionModified)node.GetChanges().Single();
        nodeModified.GradeSeparatedJunctionId.Should().Be(node.GradeSeparatedJunctionId);
        nodeModified.LowerRoadSegmentId.Should().Be(change.LowerRoadSegmentId);
        nodeModified.UpperRoadSegmentId.Should().Be(change.UpperRoadSegmentId);
        nodeModified.Type.Should().Be(change.Type);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<GradeSeparatedJunctionId>();

        var nodeAdded = Fixture.Create<GradeSeparatedJunctionAdded>();
        var node = GradeSeparatedJunction.Create(nodeAdded);
        var evt = Fixture.Create<GradeSeparatedJunctionModified>();

        // Act
        node.Apply(evt);

        // Assert
        node.GradeSeparatedJunctionId.Should().Be(evt.GradeSeparatedJunctionId);
        node.LowerRoadSegmentId.Should().Be(evt.LowerRoadSegmentId!.Value);
        node.UpperRoadSegmentId.Should().Be(evt.UpperRoadSegmentId!.Value);
        node.Type.Should().Be(evt.Type);
        node.Origin.Timestamp.Should().Be(nodeAdded.Provenance.Timestamp);
        node.Origin.OrganizationId.Should().Be(new OrganizationId(nodeAdded.Provenance.Operator));
        node.LastModified.Timestamp.Should().Be(evt.Provenance.Timestamp);
        node.LastModified.OrganizationId.Should().Be(new OrganizationId(evt.Provenance.Operator));
    }
}
