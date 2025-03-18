namespace RoadRegistry.Tests.BackOffice.Scenarios.WhenRemovingRoadSegments;

using Framework.Testing;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using RemoveRoadSegments = RoadRegistry.BackOffice.Messages.RemoveRoadSegments;

public class GivenMiniRoundabout : RemoveRoadSegmentsTestBase
{
    public GivenMiniRoundabout(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task WhenRemovingAllSegmentsButThree_ThenMiniRoundabout()
    {
        var removeRoadSegments = new RemoveRoadSegments
        {
            GeometryDrawMethod = RoadSegmentGeometryDrawMethod.Measured,
            Ids = [W6.Id]
        };

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W6.Id)
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
            Ids = [W6.Id, W7.Id]
        };

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W6.Id)
            .WithRoadSegmentRemoved(W7.Id)
            .WithRoadNodeModified(new()
            {
                Id = K6.Id,
                Type = RoadNodeType.FakeNode,
                Version = K6.Version + 1,
                Geometry = K6.Geometry
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
            Ids = [W6.Id, W7.Id, W9.Id]
        };

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W6.Id)
            .WithRoadSegmentRemoved(W7.Id)
            .WithRoadSegmentRemoved(W9.Id)
            .WithRoadNodeModified(new()
            {
                Id = K6.Id,
                Type = RoadNodeType.EndNode,
                Version = K6.Version + 1,
                Geometry = K6.Geometry
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
            Ids = [W6.Id, W7.Id, W9.Id, W10.Id]
        };

        var command = new ChangeRoadNetworkBuilder(TestData)
            .WithRemoveRoadSegments(removeRoadSegments.Ids)
            .Build();

        var expected = new RoadNetworkChangesAcceptedBuilder(TestData)
            .WithClock(Clock)
            .WithTransactionId(2)
            .WithRoadSegmentRemoved(W6.Id)
            .WithRoadSegmentRemoved(W7.Id)
            .WithRoadSegmentRemoved(W9.Id)
            .WithRoadSegmentRemoved(W10.Id)
            .WithRoadNodeRemoved(K6.Id)
            .WithRoadNodeModified(new()
            {
                Id = K5.Id,
                Type = RoadNodeType.EndNode,
                Version = K5.Version + 1,
                Geometry = K5.Geometry
            })
            .WithRoadNodeModified(new()
            {
                Id = K7.Id,
                Type = RoadNodeType.EndNode,
                Version = K7.Version + 1,
                Geometry = K7.Geometry
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
