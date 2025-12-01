namespace RoadRegistry.Tests.AggregateTests.RoadNode.AddRoadNode;

using FluentAssertions;
using Framework;
using RoadRegistry.BackOffice.Core;
using ValueObjects.Problems;

public class RoadNetworkTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenSummaryIsUpdated()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1EndNode)
                .Add(TestData.AddSegment1)
            )
            .Then((result, events) =>
            {
                result.Changes.RoadNodes.Added.Should().HaveCount(2);
            })
        );
    }

    [Fact]
    public Task WithInvalidChange_ThenProblems()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
            )
            .Then((result, events) =>
            {
                result.Problems.Should().Contain(x => x.Reason.StartsWith("RoadNode"));
            })
        );
    }

    [Fact]
    public Task WhenAddingMultipleNodesWithSameTemporaryId_ThenError()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .When(changes => changes
                .Add(TestData.AddSegment1StartNode)
                .Add(TestData.AddSegment1StartNode)
            )
            .ThenContainsProblems(new Error("RoadNodeTemporaryIdNotUnique",
                new ProblemParameter("TemporaryId", TestData.AddSegment1StartNode.TemporaryId.ToString())
            ))
        );
    }
}
