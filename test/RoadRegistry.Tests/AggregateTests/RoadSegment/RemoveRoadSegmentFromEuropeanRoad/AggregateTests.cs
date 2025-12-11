namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RemoveRoadSegmentFromEuropeanRoad;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.Events.V2;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void ThenRoadSegmentRemovedFromEuropeanRoad()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();

        var change = Fixture.Create<RemoveRoadSegmentFromEuropeanRoadChange>() with
        {
            Number = segment.Attributes.EuropeanRoadNumbers.Single()
        };

        // Act
        var problems = segment.RemoveEuropeanRoad(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var actualEvent = (RoadSegmentWasRemovedFromEuropeanRoad)segment.GetChanges().Single();
        actualEvent.RoadSegmentId.Should().Be(change.RoadSegmentId);
        actualEvent.Number.Should().Be(change.Number);
    }

    [Fact]
    public void GivenSegmentWithoutEuropeanRoad_ThenNone()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var change = Fixture.Create<RemoveRoadSegmentFromEuropeanRoadChange>() with
        {
            Number = Fixture.CreateWhichIsDifferentThan(segment.Attributes.EuropeanRoadNumbers.Single())
        };

        // Act
        var problems = segment.RemoveEuropeanRoad(change, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segmentAdded = Fixture.Create<RoadSegmentWasAdded>();
        var segment = RoadSegment.Create(segmentAdded);
        var evt = Fixture.Create<RoadSegmentWasRemovedFromEuropeanRoad>();

        // Act
        segment.Apply(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.Attributes.EuropeanRoadNumbers.Should().NotContain(evt.Number);
    }
}
