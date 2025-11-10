namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class RoadSegmentAddNationalRoadTests : RoadNetworkTestBase
{
    [Fact]
    public void ThenRoadSegmentAddedToNationalRoad()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>())
            .WithoutChanges();
        var change = Fixture.Create<AddRoadSegmentToNationalRoadChange>() with
        {
            Number = Fixture.CreateWhichIsDifferentThan(segment.Attributes.NationalRoadNumbers.Single())
        };

        // Act
        var problems = segment.AddNationalRoad(change);

        // Assert
        problems.HasError().Should().BeFalse();
        segment.GetChanges().Should().HaveCount(1);

        var actualEvent = (RoadSegmentAddedToNationalRoad)segment.GetChanges().Single();
        actualEvent.RoadSegmentId.Should().Be(change.RoadSegmentId);
        actualEvent.Number.Should().Be(change.Number);
    }

    [Fact]
    public void GivenSegmentWithNationalRoad_ThenNone()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>())
            .WithoutChanges();
        var change = Fixture.Create<AddRoadSegmentToNationalRoadChange>() with
        {
            Number = segment.Attributes.NationalRoadNumbers.Single()
        };

        // Act
        var problems = segment.AddNationalRoad(change);

        // Assert
        problems.HasError().Should().BeFalse();
        segment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public Task WhenNotFound_ThenError()
    {
        var change = Fixture.Create<AddRoadSegmentToNationalRoadChange>();

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
        var evt = Fixture.Create<RoadSegmentAddedToNationalRoad>();

        // Act
        segment.Apply(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.Attributes.NationalRoadNumbers.Should().Contain(evt.Number);
    }
}
