namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesConnectedProjection;

using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Infrastructure;
using RoadNode.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.StreetName;
using RoadSegment.Events.V2;
using Tests.AggregateTests;
using Xunit.Abstractions;
using RoadNodeAggregate = RoadRegistry.RoadNode.RoadNode;
using RoadSegmentAggregate = RoadRegistry.RoadSegment.RoadSegment;

// The read projections all run on one identity-mapped session. When an "add" event is (re)processed for an id that
// is already tracked in the same batch, the pre-fix handlers called session.Store(new ...) and Marten threw
// "Document ... with same Id already added to the session", stalling the projection. These tests reproduce that by
// replaying an add for the same id within a single batch and assert the add is applied idempotently instead.
[Collection(nameof(DockerFixtureCollection))]
public class ReadProjectionAddIdempotencyTests : IClassFixture<DatabaseFixture>
{
    private readonly DatabaseFixture _databaseFixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public ReadProjectionAddIdempotencyTests(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
    {
        _databaseFixture = databaseFixture;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task WhenRoadSegmentAddIsReplayedInTheSameBatch_ThenItIsAppliedIdempotently()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        var id = new RoadSegmentId(1);

        var firstAdd = fixture.Create<OutlinedRoadSegmentWasAdded>() with { RoadSegmentId = id, Status = RoadSegmentStatusV2.Gepland };
        var secondAdd = fixture.Create<OutlinedRoadSegmentWasAdded>() with { RoadSegmentId = id, Status = RoadSegmentStatusV2.Gerealiseerd };

        var logger = _testOutputHelper.ToLogger<RoadSegmentReadProjection>();

        await new MartenProjectionIntegrationTestRunner(_databaseFixture, logger)
            .ConfigureRoadNetworkChangesProjection(
                [new RoadSegmentReadProjection(new NullStreetNameClient(), logger)],
                options =>
                {
                    RoadSegmentReadProjection.Configure(options);
                    OrganizationReadProjection.Configure(options);
                    StreetNameReadProjection.Configure(options);
                },
                logger)
            .Given<RoadSegmentAggregate, RoadSegmentId>(id, firstAdd, secondAdd)
            .Expect(async (_, session) =>
            {
                // No "already added" was thrown (the projection completed), and the replayed add won.
                var actual = await session.LoadAsync<RoadSegmentReadItem>(id.ToInt32());
                actual.Should().NotBeNull();
                actual!.Status.Should().Be(secondAdd.Status.ToString());
            });
    }

    [Fact]
    public async Task WhenRoadNodeAddIsReplayedInTheSameBatch_ThenItIsAppliedIdempotently()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        var id = new RoadNodeId(1);

        var firstAdd = fixture.Create<RoadNodeWasAdded>() with { RoadNodeId = id, Grensknoop = false };
        var secondAdd = fixture.Create<RoadNodeWasAdded>() with { RoadNodeId = id, Grensknoop = true };

        var logger = _testOutputHelper.ToLogger<RoadNodeReadProjection>();

        await new MartenProjectionIntegrationTestRunner(_databaseFixture, logger)
            .ConfigureRoadNetworkChangesProjection(
                [new RoadNodeReadProjection()],
                RoadNodeReadProjection.Configure,
                logger)
            .Given<RoadNodeAggregate, RoadNodeId>(id, firstAdd, secondAdd)
            .Expect(async (_, session) =>
            {
                var actual = await session.LoadAsync<RoadNodeReadItem>(id.ToInt32());
                actual.Should().NotBeNull();
                actual!.Grensknoop.Should().Be(secondAdd.Grensknoop);
            });
    }

    private sealed class NullStreetNameClient : IStreetNameClient
    {
        public Task<StreetNameItem?> GetAsync(int id, CancellationToken cancellationToken)
            => Task.FromResult<StreetNameItem?>(null);
    }
}
