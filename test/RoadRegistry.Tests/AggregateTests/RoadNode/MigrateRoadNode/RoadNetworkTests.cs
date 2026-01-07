namespace RoadRegistry.Tests.AggregateTests.RoadNode.MigrateRoadNode;

using Extensions;
using FluentAssertions;
using Framework;
using RoadRegistry.RoadNode.Changes;

public class RoadNetworkTests : RoadNetworkTestBase
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
                    Type = TestData.Segment1StartNodeAdded.Type,
                    Geometry = TestData.Segment1StartNodeAdded.Geometry.ToGeometry()
                })
            )
            .Then((result, events) =>
            {
                result.Summary.RoadNodes.Modified.Should().HaveCount(1);
            })
        );
    }
}
