namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction;

using Framework;
using RoadRegistry.BackOffice.Core;

public class GradeSeparatedJunctionVerifyTopologyTests : RoadNetworkTestBase
{
    //TODO-pr test GradeSeparatedJunction.VerifyTopologyAfterChanges

    // [Fact]
    // public Task WhenNotConnectedToAnyRoadSegment_ThenError()
    // {
    //     return Run(scenario => scenario
    //         .Given(b => b)
    //         .When(changes => changes
    //             .Add(TestData.AddStartNode1)
    //         )
    //         .Throws(
    //             new Error("RoadNodeNotConnectedToAnySegment", new ProblemParameter("RoadNodeId", TestData.AddStartNode1.TemporaryId.ToString()))
    //         )
    //     );
    // }

}
