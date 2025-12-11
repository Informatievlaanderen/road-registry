namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RetireRoadSegmentBecauseOfMerger;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void WithMeasured_ThenRoadSegmentRetiredBecauseOfMerger()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentAdded>())
            .WithoutChanges();
        var mergedRoadSegmentId = segment.RoadSegmentId.Next();

        // Act
        var problems = segment.RetireBecauseOfMerger(mergedRoadSegmentId, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentRetired = (RoadSegmentRetiredBecauseOfMerger)segment.GetChanges().Single();
        segmentRetired.RoadSegmentId.Should().Be(segment.RoadSegmentId);
        segmentRetired.MergedRoadSegmentId.Should().Be(mergedRoadSegmentId);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var segmentAdded = Fixture.Create<RoadSegmentAdded>();
        var segment = RoadSegment.Create(segmentAdded);
        var evt = Fixture.Create<RoadSegmentRetiredBecauseOfMerger>() with
        {
            RoadSegmentId = segmentAdded.RoadSegmentId
        };

        // Act
        segment.Apply(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.MergedRoadSegmentId.Should().Be(evt.MergedRoadSegmentId);
        segment.Attributes.Status.Should().BeEquivalentTo(
            new RoadSegmentDynamicAttributeValues<RoadSegmentStatus>(RoadSegmentStatus.Retired));
    }

}
