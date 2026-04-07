namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RetireRoadSegment;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    [Fact]
    public void WithMeasured_ThenRoadSegmentRetired()
    {
        // Arrange
        Fixture.Freeze<RoadSegmentId>();

        var segment = RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>())
            .WithoutChanges();

        // Act
        var problems = segment.Retire(TestData.Provenance);

        // Assert
        problems.Should().HaveNoError();
        segment.GetChanges().Should().HaveCount(1);

        var segmentRetired = (RoadSegmentWasRetired)segment.GetChanges().Single();
        segmentRetired.RoadSegmentId.Should().Be(segment.RoadSegmentId);
    }

    [Fact]
    public void StateCheck()
    {
        // Arrange
        var segmentAdded = Fixture.Create<RoadSegmentWasAdded>();
        var segment = RoadSegment.Create(segmentAdded);
        var evt = Fixture.Create<RoadSegmentWasRetired>() with
        {
            RoadSegmentId = segmentAdded.RoadSegmentId
        };

        // Act
        segment.Apply(evt);

        // Assert
        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.Attributes!.Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);
    }
}
