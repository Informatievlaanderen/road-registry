namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments;

using Framework.Testing;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RemoveRoadSegments = RoadRegistry.BackOffice.Messages.RemoveRoadSegments;

public class GivenJunctions : RemoveRoadSegmentsTestBase
{
    private readonly GradeSeparatedJunctionAdded _j1;
    private readonly RoadNetworkChangesAccepted _initialRoadNetworkWithJunction;

    public GivenJunctions(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
        _j1 = new GradeSeparatedJunctionAdded
        {
            Id = 1,
            TemporaryId = 1,
            Type = GradeSeparatedJunctionType.Unknown,
            LowerRoadSegmentId = W1.Id,
            UpperRoadSegmentId = W2.Id
        };

        _initialRoadNetworkWithJunction = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithGradeSeparatedJunctionAdded(_j1)
            .WithTransactionId(2)
            .Build();
    }

    [Fact]
    public async Task WhenRemovingLowerSegment_ThenJunctionIsRemoved()
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
            .WithTransactionId(3)
            .WithGradeSeparatedJunctionRemoved(_j1.Id)
            .WithRoadSegmentRemoved(W1.Id)
            .WithRoadNodeRemoved(K1.Id)
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .Given(RoadNetworks.Stream, _initialRoadNetworkWithJunction)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }

    [Fact]
    public async Task WhenRemovingUpperSegment_ThenJunctionIsRemoved()
    {
        var removeRoadSegments = new RemoveRoadSegments
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Ids = [W2.Id]
        };

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(3)
            .WithRoadSegmentRemoved(W2.Id)
            .WithGradeSeparatedJunctionRemoved(_j1.Id)
            .Build();

        await Run(scenario =>
            scenario
                .Given(Organizations.ToStreamName(TestData.ChangedByOrganization), TestData.ChangedByImportedOrganization)
                .Given(RoadNetworks.Stream, InitialRoadNetwork)
                .Given(RoadNetworks.Stream, _initialRoadNetworkWithJunction)
                .When(command)
                .Then(RoadNetworks.Stream, expected)
        );
    }
}
