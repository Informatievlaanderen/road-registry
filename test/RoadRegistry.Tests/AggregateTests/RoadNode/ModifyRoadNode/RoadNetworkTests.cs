namespace RoadRegistry.Tests.AggregateTests.RoadNode.ModifyRoadNode;

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
                .Add(TestData.AddSegment1StartNode with { Grensknoop = true })
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1)
            )
            .When(changes => changes
                .Add(new ModifyRoadNodeChange
                {
                    RoadNodeId = TestData.Segment1StartNodeAdded.RoadNodeId,
                    Grensknoop = false
                })
            )
            .Then((result, events) =>
            {
                result.Summary.RoadNodes.Modified.Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task WhenNotFound_ThenError()
    {
        var change = Fixture.Create<ModifyRoadNodeChange>();
        change = change with { OriginalId = change.RoadNodeId.Next() };

        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(change)
            )
            .ThenProblems(new Error("RoadNodeNotFound",
                new ProblemParameter("NodeId", change.RoadNodeId.ToString()),
                new ProblemParameter("WegknoopId", change.RoadNodeId.ToString()),
                // The upload knows this road node by an identifier of its own, so the problem names that one too.
                new ProblemParameter("WegknoopOorspronkelijkeId", change.OriginalId!.Value.ToString())
            ))
        );
    }
}
