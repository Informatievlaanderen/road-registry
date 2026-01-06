namespace RoadRegistry.Tests.AggregateTests.RoadSegment.AddRoadSegmentToNationalRoad;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadSegment.Changes;
using ValueObjects.Problems;

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
                .Add(new AddRoadSegmentToNationalRoadChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    Number = Fixture.Create<NationalRoadNumber>()
                })
            )
            .Then((result, events) =>
            {
                result.Summary.RoadSegments.Modified.Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task WhenNotFound_ThenError()
    {
        var change = Fixture.Create<AddRoadSegmentToNationalRoadChange>();

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(change)
            )
            .ThenProblems(new Error("RoadSegmentNotFound", new ProblemParameter("SegmentId", change.RoadSegmentId.ToString())))
        );
    }
}
