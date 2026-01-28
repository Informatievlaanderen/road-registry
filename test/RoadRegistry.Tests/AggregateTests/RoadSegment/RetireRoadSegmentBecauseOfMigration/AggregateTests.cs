namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RetireRoadSegmentBecauseOfMigration;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void WithMeasured_ThenRoadSegmentRetiredBecauseOfMigration()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();
        var mergedRoadSegmentId = segment.RoadSegmentId.Next();

        // Act
        var problems = segment.RetireBecauseOfMigration(mergedRoadSegmentId, TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentRetired = (RoadSegmentWasRetiredBecauseOfMigration)segment.GetChanges().Single();
        segmentRetired.RoadSegmentId.Should().Be(segment.RoadSegmentId);
        segmentRetired.MergedRoadSegmentId.Should().Be(mergedRoadSegmentId);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var segmentAdded = Fixture.Create<RoadSegmentWasAdded>();
        var segment = RoadSegment.Create(segmentAdded);
        var evt = Fixture.Create<RoadSegmentWasRetiredBecauseOfMigration>() with
        {
            RoadSegmentId = segmentAdded.RoadSegmentId
        };

        // Act
        segment.Apply(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.MergedRoadSegmentId.Should().Be(evt.MergedRoadSegmentId);
        segment.Attributes.Status.Should().BeEquivalentTo(
            new RoadSegmentDynamicAttributeValues<RoadSegmentStatusV2>(RoadSegmentStatusV2.Gehistoreerd, segment.Geometry));
    }
}
