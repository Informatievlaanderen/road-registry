namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using Framework;
using RoadRegistry.BackOffice.Core;

public class RoadSegmentVerifyTopologyTests : RoadNetworkTestBase
{
    // [Fact]
    // public Task WhenStartNodeIsMissing_ThenError()
    // {
    //     var change = TestData.AddSegment1 with
    //     {
    //         StartNodeId = Fixture.CreateWhichIsDifferentThan(TestData.AddStartNode1.TemporaryId, TestData.AddEndNode1.TemporaryId)
    //     };
    //
    //     return Run(scenario => scenario
    //         .Given(changes => changes)
    //         .When(changes => changes
    //             .Add(TestData.AddStartNode1)
    //             .Add(TestData.AddEndNode1)
    //             .Add(change)
    //         )
    //         .Throws(
    //             new Error("RoadSegmentStartNodeMissing", new ProblemParameter("Identifier", change.TemporaryId.ToString()))
    //         )
    //     );
    // }

    //TODO-pr test validations RoadSegment.VerifyTopologyAfterChanges
}
