namespace RoadRegistry.Tests.AggregateTests.RoadSegment.AddRoadSegmentToEuropeanRoad;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenRoadSegmentAddedToEuropeanRoad()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>())
            .WithoutChanges();
        var change = Fixture.Create<AddRoadSegmentToEuropeanRoadChange>() with
        {
            Number = Fixture.CreateWhichIsDifferentThan(segment.Attributes.EuropeanRoadNumbers.Single())
        };

        // Act
        var problems = segment.AddEuropeanRoad(change);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var actualEvent = (RoadSegmentAddedToEuropeanRoad)segment.GetChanges().Single();
        actualEvent.RoadSegmentId.Should().Be(change.RoadSegmentId);
        actualEvent.Number.Should().Be(change.Number);
    }

    [Fact]
    public void GivenSegmentWithEuropeanRoad_ThenNone()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>())
            .WithoutChanges();
        var change = Fixture.Create<AddRoadSegmentToEuropeanRoadChange>() with
        {
            Number = segment.Attributes.EuropeanRoadNumbers.Single()
        };

        // Act
        var problems = segment.AddEuropeanRoad(change);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>());
        var evt = Fixture.Create<RoadSegmentAddedToEuropeanRoad>();

        // Act
        segment.Apply(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.Attributes.EuropeanRoadNumbers.Should().Contain(evt.Number);
    }
}
