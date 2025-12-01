namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RemoveRoadSegmentFromNationalRoad;

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
    public void ThenRoadSegmentRemovedFromNationalRoad()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>())
            .WithoutChanges();
        var change = Fixture.Create<RemoveRoadSegmentFromNationalRoadChange>() with
        {
            Number = segment.Attributes.NationalRoadNumbers.Single()
        };

        // Act
        var problems = segment.RemoveNationalRoad(change);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var actualEvent = (RoadSegmentRemovedFromNationalRoad)segment.GetChanges().Single();
        actualEvent.RoadSegmentId.Should().Be(change.RoadSegmentId);
        actualEvent.Number.Should().Be(change.Number);
    }

    [Fact]
    public void GivenSegmentWithoutNationalRoad_ThenNone()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>())
            .WithoutChanges();
        var change = Fixture.Create<RemoveRoadSegmentFromNationalRoadChange>() with
        {
            Number = Fixture.CreateWhichIsDifferentThan(segment.Attributes.NationalRoadNumbers.Single())
        };

        // Act
        var problems = segment.RemoveNationalRoad(change);

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
        var evt = Fixture.Create<RoadSegmentRemovedFromNationalRoad>();

        // Act
        segment.Apply(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.Attributes.NationalRoadNumbers.Should().NotContain(evt.Number);
    }
}
