namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments;

using Framework.Testing;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RemoveRoadSegments = RoadRegistry.BackOffice.Messages.RemoveRoadSegments;

public class GivenEndNode : RemoveRoadSegmentsTestBase
{
    public GivenEndNode(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    { }

    [Fact]
    public async Task WhenRemovingSingleSegment_ThenNodeIsRemoved()
    {
        var removeRoadSegments = new RemoveRoadSegments
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Ids = [W1.Id]
        };

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W1.Id)
            .WithRoadNodeRemoved(K1.Id)
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
