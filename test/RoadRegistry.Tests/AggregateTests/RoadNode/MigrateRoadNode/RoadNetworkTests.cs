namespace RoadRegistry.Tests.AggregateTests.RoadNode.MigrateRoadNode;

using FluentAssertions;
using Framework;
using RoadRegistry.RoadNode.Changes;

public class ScopedRoadNetworkTests : RoadNetworkTestBase
{
    [Fact]
    public Task ThenSummaryIsUpdated()
    {
        return Run(scenario => scenario
            .Given(given => given)
            .WhenMigrate(changes => changes
                .Add(new ModifyRoadNodeChange
                {
                    RoadNodeId = TestData.Segment1StartNodeAdded.RoadNodeId,
                    Geometry = TestData.Segment1StartNodeAdded.Geometry,
                    Grensknoop = TestData.Segment1StartNodeAdded.Grensknoop
                })
            )
            .Then((result, events) =>
            {
                result.Summary.RoadNodes.Modified.Should().HaveCount(1);
            })
        );
    }
}
