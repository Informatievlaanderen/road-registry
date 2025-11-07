namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using Framework;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadNetwork.Changes;

public class RoadSegmentAddTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenRoadSegmentAdded()
    {
        return Run(scenario => scenario
                .Given(changes => changes)
                .When(changes => changes
                    .Add(TestData.AddStartNode1)
                    .Add(TestData.AddEndNode1)
                    .Add(TestData.AddSegment1))
                .Then(
                    TestData.StartNode1Added,
                    TestData.EndNode1Added,
                    TestData.Segment1Added
                )
        );
    }

    [Fact]
    public Task WhenStartNodeIsMissing_ThenError()
    {
        var change = new ModifyRoadSegmentChange
        {
            Id = TestData.Segment1Added.Id,
            StartNodeId = new RoadNodeId(9)
        };

        return Run(scenario => scenario
            .Given(changes => changes
                .Add(TestData.AddStartNode1)
                .Add(TestData.AddEndNode1)
                .Add(TestData.AddSegment1)
            )
            .When(changes => changes.Add(change))
            .Throws(new Error("RoadSegmentStartNodeMissing", [new("Identifier", "1")]))
        );
    }

    //TODO-pr test validations RoadSegment.Add
}
