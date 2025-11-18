namespace RoadRegistry.Tests.AggregateTests.RoadNode;

using Framework;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadNode.Changes;

public class RoadNodeVerifyTopologyTests : RoadNetworkTestBase
{
    [Fact]
    public Task WhenRemovedAndSegmentsAreStillConnectedByStartNode_ThenError()
    {
        return Run(scenario => scenario
            .Given(b => b
                .Add(TestData.AddStartNode1)
                .Add(TestData.AddEndNode1)
                .Add(TestData.AddSegment1)
            )
            .When(changes => changes
                .Add(new RemoveRoadNodeChange
                {
                    Id = TestData.StartNode1Added.RoadNodeId
                })
            )
            .ThenProblems(new Error("RoadSegmentStartNodeMissing", new ProblemParameter("Identifier", "1")))
        );
    }
    [Fact]
    public Task WhenRemovedAndSegmentsAreStillConnectedByEndNode_ThenError()
    {
        return Run(scenario => scenario
            .Given(b => b
                .Add(TestData.AddStartNode1)
                .Add(TestData.AddEndNode1)
                .Add(TestData.AddSegment1)
            )
            .When(changes => changes
                .Add(new RemoveRoadNodeChange
                {
                    Id = TestData.EndNode1Added.RoadNodeId
                })
            )
            .ThenProblems(new Error("RoadSegmentEndNodeMissing", new ProblemParameter("Identifier", "1")))
        );
    }

    [Fact]
    public Task WhenGeometryIsTooCloseToOtherRoadNode_ThenError()
    {
        return Run(scenario => scenario
            .Given(g => g)
            .When(changes => changes
                .Add(TestData.AddStartNode1)
                .Add(TestData.AddEndNode1 with
                {
                    Geometry = new Point(TestData.AddStartNode1.Geometry.X + 0.0001, TestData.AddStartNode1.Geometry.Y)
                })
            )
            .ThenContainsProblems(new Error("RoadNodeGeometryTaken", new ProblemParameter("ByOtherNode", TestData.AddStartNode1.TemporaryId.ToString())))
        );
    }

    [Fact]
    public Task WhenNotConnectedToAnyRoadSegment_ThenError()
    {
        return Run(scenario => scenario
            .Given(b => b)
            .When(changes => changes
                .Add(TestData.AddStartNode1)
            )
            .ThenProblems(new Error("RoadNodeNotConnectedToAnySegment", new ProblemParameter("RoadNodeId", TestData.AddStartNode1.TemporaryId.ToString())))
        );
    }

    //TODO-pr test RoadNode.VerifyTopology
}
