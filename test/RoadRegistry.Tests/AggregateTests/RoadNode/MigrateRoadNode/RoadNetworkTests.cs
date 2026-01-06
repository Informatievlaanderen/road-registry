namespace RoadRegistry.Tests.AggregateTests.RoadNode.MigrateRoadNode;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadNode.Changes;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects.Problems;

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
            .WhenMigrate(changes => changes
                .Add(new ModifyRoadNodeChange
                {
                    RoadNodeId = TestData.Segment1StartNodeAdded.RoadNodeId,
                    Type = TestData.Segment1StartNodeAdded.Type,
                    Geometry = null
                })
            )
            .Then((result, events) =>
            {
                result.Problems.HasError().Should().BeFalse();
                result.Summary.RoadNodes.Modified.Should().HaveCount(1);
            })
        );
    }

    [Fact]
    public Task WhenNotFound_ThenError()
    {
        var change = Fixture.Create<ModifyRoadNodeChange>();

        return Run(scenario => scenario
            .Given(given => given)
            .WhenMigrate(changes => changes
                .Add(change)
            )
            .ThenProblems(new Error("RoadNodeNotFound", new ProblemParameter("NodeId", change.RoadNodeId.ToString())))
        );
    }
}
