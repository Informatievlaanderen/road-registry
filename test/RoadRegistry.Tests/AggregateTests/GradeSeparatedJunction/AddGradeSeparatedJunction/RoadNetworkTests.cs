namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction.AddGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using Framework;
using NetTopologySuite.Geometries;
using RoadRegistry.GradeSeparatedJunction.Changes;

public class RoadNetworkTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenSummaryIsUpdated()
    {
        var segment1Start = new Point(0, 0);
        var segment1End = new Point(10, 0);
        var segment2Start = new Point(5, 5);
        var segment2End = new Point(5, -5);

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode with
                {
                    Geometry = segment1Start
                })
                .Add(TestData.AddSegment1EndNode with
                {
                    Geometry = segment1End
                })
                .Add(TestData.AddSegment1 with
                {
                    Geometry = BuildSegmentGeometry(segment1Start, segment1End)
                })
                .Add(TestData.AddSegment2StartNode with
                {
                    Geometry = segment2Start
                })
                .Add(TestData.AddSegment2EndNode with
                {
                    Geometry = segment2End
                })
                .Add(TestData.AddSegment2 with
                {
                    Geometry = BuildSegmentGeometry(segment2Start, segment2End)
                })
                .Add(Fixture.Create<AddGradeSeparatedJunctionChange>() with
                {
                    LowerRoadSegmentId = TestData.AddSegment1.TemporaryId,
                    UpperRoadSegmentId = TestData.AddSegment2.TemporaryId
                })
            )
            .Then((result, events) =>
            {
                result.Summary.GradeSeparatedJunctions.Added.Should().HaveCount(1);
            })
        );
    }
}
