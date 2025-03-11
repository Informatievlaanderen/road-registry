namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments;

using Framework.Testing;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RemoveRoadSegments = RoadRegistry.BackOffice.Messages.RemoveRoadSegments;

public class GivenTurningLoopNode : RemoveRoadSegmentsTestBase
{
    public GivenTurningLoopNode(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    { }

    [Fact]
    public async Task WhenRemovingSingleSegment_ThenEndNode()
    {
        var removeRoadSegments = new RemoveRoadSegments
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Ids = [W3.Id]
        };

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentsRemoved(new RoadSegmentsRemoved
            {
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                RemovedRoadSegmentIds = [W3.Id],
                RemovedRoadNodeIds = [],
                ChangedRoadNodes = [
                new RoadNodeTypeChanged
                {
                    Id = K4.Id,
                    Type = RoadNodeType.EndNode,
                    Version = K4.Version + 1
                }],
                RemovedGradeSeparatedJunctionIds = []
            })
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }

    [Fact]
    public async Task WhenRemovingBothSegments_ThenNodeIsRemoved()
    {
        var removeRoadSegments = new RemoveRoadSegments
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Ids = [W3.Id, W4.Id]
        };

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentsRemoved(new RoadSegmentsRemoved
            {
                GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
                RemovedRoadSegmentIds = [W3.Id, W4.Id],
                RemovedRoadNodeIds = [K4.Id],
                ChangedRoadNodes = [],
                RemovedGradeSeparatedJunctionIds = []
            })
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }
}
