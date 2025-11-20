namespace RoadRegistry.Tests.AggregateTests.RoadNode.RemoveRoadNode;

using AutoFixture;
using FluentAssertions;
using Framework;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadNode.Changes;

public class RoadNetworkTests : RoadNetworkTestBase
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
                result.Changes.RoadNodes.Removed.Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task WhenNotFound_ThenError()
    {
        var change = Fixture.Create<RemoveRoadNodeChange>();

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(change)
            )
            .ThenProblems(new Error("RoadNodeNotFound", new ProblemParameter("NodeId", change.RoadNodeId.ToString())))
        );
    }
}
