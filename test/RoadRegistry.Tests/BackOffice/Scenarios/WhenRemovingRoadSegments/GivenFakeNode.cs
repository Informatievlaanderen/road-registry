namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments;

using Framework.Testing;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RemoveRoadSegments = RoadRegistry.BackOffice.Messages.RemoveRoadSegments;

public class GivenFakeNode : RemoveRoadSegmentsTestBase
{
    public GivenFakeNode(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    { }

    [Fact]
    public async Task WhenRemovingSingleSegment_ThenEndNode()
    {
        var removeRoadSegments = new RemoveRoadSegments
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Ids = [W5.Id]
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
                RemovedRoadSegmentIds = [W5.Id],
                RemovedRoadNodeIds = [],
                ChangedRoadNodes = [new RoadNodeTypeChanged
                {
                    Id = K5.Id,
                    Type = RoadNodeType.EndNode,
                    Version = K5.Version + 1
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
            Ids = [W5.Id, W9.Id]
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
                RemovedRoadSegmentIds = [W5.Id, W9.Id],
                RemovedRoadNodeIds = [K5.Id],
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
