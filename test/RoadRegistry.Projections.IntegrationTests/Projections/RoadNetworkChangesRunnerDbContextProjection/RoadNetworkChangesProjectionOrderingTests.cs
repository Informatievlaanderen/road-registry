namespace RoadRegistry.Projections.IntegrationTests.Projections.RoadNetworkChangesRunnerDbContextProjection;

using System.Collections.Concurrent;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
using FluentAssertions;
using Infrastructure;
using JasperFx.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using RoadSegment.Events.V2;
using Tests.AggregateTests;

// A DbContext-backed projection takes a batch to be exactly the page the daemon delivered - it never fetches a
// correlation's later events into the batch, and it orders what it has by seq_id rather than by emission ordinal
// (RoadNetworkChangesProjection.RequiresEmissionOrder is false for that driver).
//
// This test holds that to its promise: the same correlation, the same two events, once inside a single page and once
// split over several, must reach the sub-projections in the same order. It is the property the driver's idempotency
// rests on - a batch that never reaches past its page is a batch whose position covers everything it applied.
//
// Before that change the two runs disagreed: whole in one page the events came back in emission order, split up in
// stored order, and which one you got depended on whether the projection happened to be catching up - which it is
// after every restart with a backlog.
[Collection(nameof(DockerFixtureCollection))]
public class RoadNetworkChangesProjectionOrderingTests : IClassFixture<DatabaseFixture>
{
    private const string Added = "added";
    private const string Modified = "modified";

    private readonly DatabaseFixture _databaseFixture;
    private readonly IFixture _fixture = new RoadNetworkTestDataV2().Fixture;

    public RoadNetworkChangesProjectionOrderingTests(DatabaseFixture databaseFixture)
    {
        _databaseFixture = databaseFixture;
    }

    [Fact]
    public async Task TheAppliedOrderIsTheSameWhetherTheCorrelationFitsInOnePageOrIsSplitOverPages()
    {
        var inOnePage = await Replay(batchSize: 5000);
        var splitOverPages = await Replay(batchSize: 1);

        splitOverPages.Should().Equal(inOnePage,
            "where the page boundaries fall may not decide the order the events are applied in");
        inOnePage.Should().Equal([Modified, Added],
            "the events are applied in the order Marten stored them, which is the same for any page size");
    }

    // One correlation holding two events whose stored order is the reverse of their emission order: the modification is
    // stored first but was raised second (ordinal 2), the add is stored second but was raised first (ordinal 1). That
    // inversion is what the ordinal header is for, and it is what a road network change produces - events appended to
    // existing streams and events starting new streams do not come back in the order they were raised.
    private async Task<string[]> Replay(int batchSize)
    {
        var recorder = new AppliedOrderProjection();
        var dbContextFactory = CreateDbContextFactory();

        var modified = _fixture.Create<RoadSegmentWasModified>();
        var added = _fixture.Create<RoadSegmentWasAdded>();

        await new MartenProjectionIntegrationTestRunner(_databaseFixture)
            .ConfigureServices(services => services.AddSingleton(dbContextFactory))
            .ConfigureRoadNetworkChangesProjection(
                new TestDbContextRoadNetworkChangesProjection(dbContextFactory, [
                    new TestDbContextRunnerDbContextProjection([recorder])
                ], batchSize))
            .GivenWithOrdinals([
                (StreamKeyFactory.Create(typeof(RoadRegistry.RoadSegment.RoadSegment), modified.RoadSegmentId), modified, 2L),
                (StreamKeyFactory.Create(typeof(RoadRegistry.RoadSegment.RoadSegment), added.RoadSegmentId), added, 1L)
            ])
            .Expect((_, _) => Task.CompletedTask);

        return recorder.Applied;
    }

    private static IDbContextFactory<TestDbContext> CreateDbContextFactory()
    {
        var database = Guid.NewGuid().ToString();

        var dbContextFactory = new Mock<IDbContextFactory<TestDbContext>>();
        dbContextFactory
            .Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var builder = new DbContextOptionsBuilder<TestDbContext>()
                    .UseInMemoryDatabase(database);

                return new TestDbContext(builder.Options);
            });
        return dbContextFactory.Object;
    }

    // Writes nothing; it only records the order in which the events reached a sub-projection. The daemon applies them
    // on its own thread, hence the concurrent queue.
    private sealed class AppliedOrderProjection : ConnectedProjection<TestDbContext>
    {
        private readonly ConcurrentQueue<string> _applied = new();

        public string[] Applied => _applied.ToArray();

        public AppliedOrderProjection()
        {
            When<IEvent<RoadSegmentWasAdded>>((_, _, _) =>
            {
                _applied.Enqueue(Added);
                return Task.CompletedTask;
            });

            When<IEvent<RoadSegmentWasModified>>((_, _, _) =>
            {
                _applied.Enqueue(Modified);
                return Task.CompletedTask;
            });
        }
    }
}
