namespace RoadRegistry.Tests.AggregateTests.RoadNode;

using Framework;

public class RoadNodeAddTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenRoadNodeAdded()
    {
        return Run(scenario => scenario
                .Given(b => b)
                .When(changes => changes
                    .Add(TestData.AddStartNode1)
                    .Add(TestData.AddEndNode1)
                    .Add(TestData.AddSegment1)
                )
                .Then(
                    TestData.StartNode1Added,
                    TestData.EndNode1Added,
                    TestData.Segment1Added
                )
        );
    }

    //TODO-pr test validations RoadNode.Add
}
