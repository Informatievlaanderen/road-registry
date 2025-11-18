namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class RoadSegmentRemoveEuropeanRoadTests : RoadNetworkTestBase
{
    [Fact]
    public void ThenRoadSegmentRemovedFromEuropeanRoad()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>())
            .WithoutChanges();

        var change = Fixture.Create<RemoveRoadSegmentFromEuropeanRoadChange>() with
        {
            Number = segment.Attributes.EuropeanRoadNumbers.Single()
        };

        // Act
        var problems = segment.RemoveEuropeanRoad(change);

        // Assert
        problems.HasError().Should().BeFalse();
        segment.GetChanges().Should().HaveCount(1);

        var actualEvent = (RoadSegmentRemovedFromEuropeanRoad)segment.GetChanges().Single();
        actualEvent.RoadSegmentId.Should().Be(change.RoadSegmentId);
        actualEvent.Number.Should().Be(change.Number);
    }

    [Fact]
    public void GivenSegmentWithoutEuropeanRoad_ThenNone()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>())
            .WithoutChanges();
        var change = Fixture.Create<RemoveRoadSegmentFromEuropeanRoadChange>() with
        {
            Number = Fixture.CreateWhichIsDifferentThan(segment.Attributes.EuropeanRoadNumbers.Single())
        };

        // Act
        var problems = segment.RemoveEuropeanRoad(change);

        // Assert
        problems.HasError().Should().BeFalse();
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public Task WhenNotFound_ThenError()
    {
        var change = Fixture.Create<RemoveRoadSegmentFromEuropeanRoadChange>();

        return Run(scenario => ScenarioExtensions.ThenProblems(scenario
                .Given(changes => changes)
                .When(changes => changes.Add(change)), new Error("RoadSegmentNotFound", [new("SegmentId", change.RoadSegmentId.ToString())]))
        );
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>());
        var evt = Fixture.Create<RoadSegmentRemovedFromEuropeanRoad>();

        // Act
        segment.Apply(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.Attributes.EuropeanRoadNumbers.Should().NotContain(evt.Number);
    }
}
