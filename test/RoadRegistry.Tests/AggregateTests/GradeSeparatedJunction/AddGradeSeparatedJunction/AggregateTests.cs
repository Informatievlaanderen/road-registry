namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.AddGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.ValueObjects;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenGradeSeparatedJunctionAdded()
    {
        // Arrange
        var change = Fixture.Create<AddGradeSeparatedJunctionChange>();
        var geometry = Fixture.Create<JunctionGeometry>();

        // Act
        var (junction, problems) = GradeSeparatedJunction.Add(change, geometry, TestData.Provenance, new InMemoryRoadNetworkIdGenerator());

        // Assert
        problems.Should().HaveNoError();
        junction.GetChanges().Should().HaveCount(1);

        var junctionAdded = (GradeSeparatedJunctionWasAdded)junction.GetChanges().Single();
        junctionAdded.GradeSeparatedJunctionId.Should().Be(new GradeSeparatedJunctionId(1));
        junctionAdded.Type.Should().Be(change.Type);
        junctionAdded.LowerRoadSegmentId.Should().Be(change.LowerRoadSegmentId);
        junctionAdded.UpperRoadSegmentId.Should().Be(change.UpperRoadSegmentId);
        junctionAdded.Geometry.Should().Be(geometry);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var evt = Fixture.Create<GradeSeparatedJunctionWasAdded>();

        // Act
        var junction = GradeSeparatedJunction.Create(evt);

        // Assert
        junction.GradeSeparatedJunctionId.Should().Be(evt.GradeSeparatedJunctionId);
        junction.Type.Should().Be(evt.Type);
        junction.LowerRoadSegmentId.Should().Be(evt.LowerRoadSegmentId);
        junction.UpperRoadSegmentId.Should().Be(evt.UpperRoadSegmentId);
    }
}
