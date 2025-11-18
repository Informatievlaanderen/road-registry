namespace RoadRegistry.Tests.AggregateTests.GradeSeparatedJunction;

using AutoFixture;
using Framework;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.GradeSeparatedJunction.Changes;

public class GradeSeparatedJunctionVerifyTopologyTests : RoadNetworkTestBase
{
    //TODO-pr test GradeSeparatedJunction.VerifyTopology
    /*      if (!context.RoadNetwork.RoadSegments.TryGetValue(UpperRoadSegmentId, out var upperSegment))
        {
            problems = problems.Add(new UpperRoadSegmentMissing());
        }

        if (!context.RoadNetwork.RoadSegments.TryGetValue(LowerRoadSegmentId, out var lowerSegment))
        {
            problems = problems.Add(new LowerRoadSegmentMissing());
        }

        if (upperSegment is not null
            && lowerSegment is not null
            && !upperSegment.Geometry.Intersects(lowerSegment.Geometry))
        {
            problems = problems.Add(new UpperAndLowerRoadSegmentDoNotIntersect());
        }*/

    [Fact]
    public Task WhenUpperRoadSegmentIsMissing_ThenError()
    {
        return Run(scenario => ScenarioExtensions.ThenProblems(scenario
                    .Given(b => b)
                    .When(changes => changes
                        .Add(TestData.AddSegment1StartNode)
                        .Add(TestData.AddSegment1EndNode)
                        .Add(TestData.AddSegment1)
                        .Add(Fixture.Create<AddGradeSeparatedJunctionChange>() with
                        {
                            LowerRoadSegmentId = TestData.Segment1Added.RoadSegmentId
                        })
                    ), new UpperRoadSegmentMissing()
            )
        );
    }

}
