namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class RoadSegmentAddEuropeanRoadTests : RoadNetworkTestBase
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
        problems.HasError().Should().BeFalse();
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
        problems.HasError().Should().BeFalse();
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public Task WhenNotFound_ThenError()
    {
        var change = Fixture.Create<AddRoadSegmentToEuropeanRoadChange>();

        return Run(scenario => scenario
            .Given(changes => changes)
            .When(changes => changes.Add(change))
            .Throws(new Error("RoadSegmentNotFound", [new("SegmentId", change.RoadSegmentId.ToString())]))
        );
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
