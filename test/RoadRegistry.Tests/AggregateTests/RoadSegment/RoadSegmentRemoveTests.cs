namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using Framework;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadSegmentModified = RoadRegistry.RoadSegment.Events.RoadSegmentModified;

public class RoadSegmentRemoveTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenRoadSegmentRemoved()
    {
        var change = new RemoveRoadSegmentChange
        {
            RoadSegmentId = TestData.Segment1Added.RoadSegmentId
        };

        return Run(scenario => scenario
                .Given(changes => changes
                    .Add(TestData.AddStartNode1)
                    .Add(TestData.AddEndNode1)
                    .Add(TestData.AddSegment1)
                )
                .When(changes => changes.Add(change))
                .Then(new RoadSegmentRemoved
                {
                    RoadSegmentId = change.RoadSegmentId
                })
        );
    }

    [Fact]
    public Task WhenNotFound_ThenError()
    {
        var change = new RemoveRoadSegmentChange
        {
            RoadSegmentId = TestData.Segment1Added.RoadSegmentId
        };

        return Run(scenario => scenario
            .Given(changes => changes)
            .When(changes => changes.Add(change))
            .Throws(new Error("RoadSegmentNotFound", [new("SegmentId", change.RoadSegmentId.ToString())]))
        );
    }
}
