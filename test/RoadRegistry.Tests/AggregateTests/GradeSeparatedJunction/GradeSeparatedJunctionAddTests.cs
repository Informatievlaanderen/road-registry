namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.BackOffice;
using RoadRegistry.GradeSeparatedJunction;
using RoadRegistry.GradeSeparatedJunction.Changes;
using RoadRegistry.GradeSeparatedJunction.Events;

public class GradeSeparatedJunctionAddTests : RoadNetworkTestBase
{
    [Fact]
    public void ThenGradeSeparatedJunctionAdded()
    {
        // Arrange
        var change = Fixture.Create<AddGradeSeparatedJunctionChange>();

        // Act
        var (junction, problems) = GradeSeparatedJunction.Add(change, new FakeRoadNetworkIdGenerator());

        // Assert
        problems.HasError().Should().BeFalse();
        junction.GetChanges().Should().HaveCount(1);

        var junctionAdded = (GradeSeparatedJunctionAdded)junction.GetChanges().Single();
        junctionAdded.GradeSeparatedJunctionId.Should().Be(new GradeSeparatedJunctionId(1));
        junctionAdded.Type.Should().Be(change.Type);
        junctionAdded.LowerRoadSegmentId.Should().Be(change.LowerRoadSegmentId);
        junctionAdded.UpperRoadSegmentId.Should().Be(change.UpperRoadSegmentId);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var evt = Fixture.Create<GradeSeparatedJunctionAdded>();

        // Act
        var junction = GradeSeparatedJunction.Create(evt);

        // Assert
        junction.GradeSeparatedJunctionId.Should().Be(evt.GradeSeparatedJunctionId);
        junction.Type.Should().Be(evt.Type);
        junction.LowerRoadSegmentId.Should().Be(evt.LowerRoadSegmentId);
        junction.UpperRoadSegmentId.Should().Be(evt.UpperRoadSegmentId);
    }
}
