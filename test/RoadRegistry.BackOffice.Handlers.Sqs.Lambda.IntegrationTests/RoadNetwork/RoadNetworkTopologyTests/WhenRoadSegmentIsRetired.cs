namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.RoadNetworkTopologyTests;

using AutoFixture;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.RoadSegment.Events.V2;
using RoadSegment;
using ScopedRoadNetwork;
using ScopedRoadNetwork.ValueObjects;
using Xunit.Abstractions;

// A road segment retired by a merger or a split is historized, not removed. The topology projection used to delete it,
// so it went missing from every topology lookup; the aggregate keeps it with its geometry and without road nodes, like
// a segment historized from realized, and the lookups now read the aggregate.
[Collection(nameof(DockerFixtureCollection))]
public class WhenRoadSegmentIsRetired : RoadNetworkIntegrationTest
{
    public WhenRoadSegmentIsRetired(DatabaseFixture databaseFixture, ITestOutputHelper testOutputHelper)
        : base(databaseFixture, testOutputHelper)
    {
    }

    [Theory]
    [InlineData(nameof(RoadSegmentWasRetiredBecauseOfMerger))]
    [InlineData(nameof(RoadSegmentWasRetiredBecauseOfSplit))]
    public async Task ThenTheSegmentStaysInTheTopologyWithoutRoadNodes(string retiredEvent)
    {
        var sp = await BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();

        await AddRoadSegment(store, 1);
        await Append(store, 1, Retired(retiredEvent, 1));

        var ids = await LoadUnderlyingIds(sp);

        ids.RoadSegmentIds.Should().BeEquivalentTo([new RoadSegmentId(1)]);
        ids.RoadNodeIds.Should().BeEmpty();
    }

    private object Retired(string retiredEvent, int roadSegmentId)
    {
        return retiredEvent switch
        {
            nameof(RoadSegmentWasRetiredBecauseOfMerger) => TestData.Fixture.Create<RoadSegmentWasRetiredBecauseOfMerger>() with
            {
                RoadSegmentId = new RoadSegmentId(roadSegmentId),
                MergedRoadSegmentId = new RoadSegmentId(100)
            },
            nameof(RoadSegmentWasRetiredBecauseOfSplit) => TestData.Fixture.Create<RoadSegmentWasRetiredBecauseOfSplit>() with
            {
                RoadSegmentId = new RoadSegmentId(roadSegmentId)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(retiredEvent))
        };
    }

    private Task AddRoadSegment(IDocumentStore store, int roadSegmentId)
    {
        return Append(store, roadSegmentId, TestData.Fixture.Create<RoadSegmentWasAdded>() with
        {
            RoadSegmentId = new RoadSegmentId(roadSegmentId),
            Geometry = BuildRoadSegmentGeometry(0, 0, 10, 0),
            StartNodeId = new RoadNodeId(roadSegmentId * 10 + 1),
            EndNodeId = new RoadNodeId(roadSegmentId * 10 + 2)
        });
    }

    // One session per event: the topology functions compare event timestamps, and events saved together share one.
    private static async Task Append(IDocumentStore store, int roadSegmentId, object @event)
    {
        await using var session = store.LightweightSession();
        session.Events.AppendOrStartStream(StreamKeyFactory.Create(typeof(RoadSegment), new RoadSegmentId(roadSegmentId)), @event);
        await session.SaveChangesAsync();
    }

    private static async Task<RoadNetworkIds> LoadUnderlyingIds(IServiceProvider sp)
    {
        var store = sp.GetRequiredService<IDocumentStore>();
        var repository = sp.GetRequiredService<IRoadNetworkRepository>();

        await using var session = store.LightweightSession();
        return IdsOf(await repository.LoadUnderlying(session, new ScopedRoadNetworkId(Guid.NewGuid()), BuildRoadSegmentGeometry(5, -5, 5, 5).Value));
    }
}
