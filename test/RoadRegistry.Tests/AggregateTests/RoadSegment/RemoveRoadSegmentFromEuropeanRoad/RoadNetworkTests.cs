namespace RoadRegistry.Tests.AggregateTests.RoadSegment.RemoveRoadSegmentFromEuropeanRoad;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class RoadNetworkTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenSummaryIsUpdated()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1))
            .When(changes => changes
                .Add(new RemoveRoadSegmentFromEuropeanRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = TestData.Segment1Added.EuropeanRoadNumbers.First()
                })
            )
            .Then((result, events) =>
            {
                result.Changes.RoadSegments.Modified.Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task WhenNotFound_ThenError()
    {
        var change = Fixture.Create<RemoveRoadSegmentFromEuropeanRoadChange>();

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(change)
            )
            .ThenProblems(new Error("RoadSegmentNotFound", new ProblemParameter("SegmentId", change.RoadSegmentId.ToString())))
        );
    }
}
