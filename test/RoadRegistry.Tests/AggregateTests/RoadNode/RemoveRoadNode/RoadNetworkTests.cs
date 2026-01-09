namespace RoadRegistry.Tests.AggregateTests.RoadNode.RemoveRoadNode;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadNode.Changes;
using ValueObjects.Problems;

public class ScopedRoadNetworkTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenSummaryIsUpdated()
    {
        return Run(scenario => scenario
            .Given(given => given
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1)
            )
            .When(changes => changes
                .Add(new RemoveRoadNodeChange
                {
                    RoadNodeId = TestData.Segment1StartNodeAdded.RoadNodeId
                })
            )
            .Then((result, events) =>
            {
                result.Summary.RoadNodes.Removed.Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task WhenNotFound_ThenNoProblem()
    {
        var change = Fixture.Create<RemoveRoadNodeChange>();

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(change)
            )
            .Then((result, events) =>
            {
                result.Problems.HasError().Should().BeFalse();
            })
        );
    }
}
