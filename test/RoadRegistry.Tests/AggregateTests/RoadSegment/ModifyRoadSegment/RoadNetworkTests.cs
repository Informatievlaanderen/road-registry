namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ModifyRoadSegment;

using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadNetwork;
using RoadRegistry.RoadSegment;
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
                .Add(new ModifyRoadSegmentChange
                {
                    RoadSegmentId = TestData.Segment1Added.RoadSegmentId,
                    OriginalId = TestData.Segment1Added.RoadSegmentId
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
        var change = Fixture.Create<ModifyRoadSegmentChange>();

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(change)
            )
            .ThenProblems(new Error("RoadSegmentNotFound", new ProblemParameter("SegmentId", change.OriginalId.ToString())))
        );
    }
}
