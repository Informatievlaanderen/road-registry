namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments;

using Framework.Testing;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RemoveRoadSegments = RoadRegistry.BackOffice.Messages.RemoveRoadSegments;

public class GivenRealNode : RemoveRoadSegmentsTestBase
{
    public GivenRealNode(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task WhenRemovingAllSegmentsButThree_ThenRealNode()
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

    [Fact]
    public async Task WhenRemovingAllSegmentsButTwo_ThenFakeNode()
    {
        var removeRoadSegments = new RemoveRoadSegments
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Ids = [W1.Id, W2.Id]
        };

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W1.Id)
            .WithRoadSegmentRemoved(W2.Id)
            .WithRoadNodeRemoved(K1.Id)
            .WithRoadNodeModified(new()
            {
                Id = K2.Id,
                Type = RoadNodeType.FakeNode,
                Version = K2.Version + 1,
                Geometry = K2.Geometry
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
    public async Task WhenRemovingAllSegmentsButOne_ThenEndNode()
    {
        var removeRoadSegments = new RemoveRoadSegments
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Ids = [W1.Id, W2.Id, W5.Id]
        };

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W1.Id)
            .WithRoadSegmentRemoved(W2.Id)
            .WithRoadSegmentRemoved(W5.Id)
            .WithRoadNodeRemoved(K1.Id)
            .WithRoadNodeModified(new()
            {
                Id = K2.Id,
                Type = RoadNodeType.EndNode,
                Version = K2.Version + 1,
                Geometry = K2.Geometry
            })
            .WithRoadNodeModified(new()
            {
                Id = K5.Id,
                Type = RoadNodeType.EndNode,
                Version = K5.Version + 1,
                Geometry = K5.Geometry
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
    public async Task WhenRemovingAllSegments_ThenNodeIsRemoved()
    {
        var removeRoadSegments = new RemoveRoadSegments
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Ids = [W1.Id, W2.Id, W5.Id, W6.Id]
        };

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W1.Id)
            .WithRoadSegmentRemoved(W2.Id)
            .WithRoadSegmentRemoved(W5.Id)
            .WithRoadSegmentRemoved(W6.Id)
            .WithRoadNodeRemoved(K1.Id)
            .WithRoadNodeRemoved(K2.Id)
            .WithRoadNodeModified(new()
            {
                Id = K5.Id,
                Type = RoadNodeType.EndNode,
                Version = K5.Version + 1,
                Geometry = K5.Geometry
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
