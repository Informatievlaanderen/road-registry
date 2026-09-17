namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RoadNetwork.RoadNetworkTopologyTests;

using AutoFixture;
using Dapper;
using FluentAssertions;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.RoadSegment.Events.V2;
using RoadSegment;
using ScopedRoadNetwork;
using Xunit.Abstractions;

// A road segment retired by a merger or a split is historized, not removed. The topology used to delete it, so it went
// missing from every topology lookup; it now keeps the segment with its geometry and without road nodes, like a segment
// historized from realized. 20260917120000_networktopology_restore_retired_roadsegments.sql puts back the rows the
// projection deleted before.
[Collection(nameof(DockerFixtureCollection))]
public class WhenRoadSegmentIsRetired : RoadNetworkIntegrationTest
{
    private const string DataFixMigration = "20260917120000_networktopology_restore_retired_roadsegments.sql";

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

        var ids = await GetUnderlyingIds(sp);

        ids.RoadSegmentIds.Should().BeEquivalentTo([new RoadSegmentId(1)]);
        ids.RoadNodeIds.Should().BeEmpty();
    }

    [Fact]
    public async Task GivenTheProjectionDeletedTheRow_WhenTheDataFixRuns_ThenTheRowIsRestoredFromTheAggregate()
    {
        var sp = await BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();

        await AddRoadSegment(store, 1);
        await Append(store, 1, Retired(nameof(RoadSegmentWasRetiredBecauseOfMerger), 1));
        await AddRoadSegment(store, 2);
        await Append(store, 2, Retired(nameof(RoadSegmentWasRetiredBecauseOfSplit), 2));
        await AddRoadSegment(store, 3);
        await Append(store, 3, TestData.Fixture.Create<RoadSegmentWasRemoved>() with { RoadSegmentId = new RoadSegmentId(3) });
        await AddRoadSegment(store, 4);

        // What the projection did before: delete the retired segments.
        await Execute(store, $"DELETE FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName} WHERE id IN (1, 2);");
        var untouchedBefore = await QueryTopology(store);

        await Execute(store, ReadDataFixMigration());
        await Execute(store, ReadDataFixMigration());

        var topology = await QueryTopology(store);
        topology.Select(x => x.Id).Should().BeEquivalentTo([1, 2, 4], "a removed segment stays out, and running the fix twice adds nothing");

        foreach (var restored in topology.Where(x => x.Id is 1 or 2))
        {
            restored.StartNodeId.Should().BeNull();
            restored.EndNodeId.Should().BeNull();
            restored.IsV2.Should().BeTrue();
            restored.Wkt.Should().Be(untouchedBefore.Single(x => x.Id == 4).Wkt, "the geometry comes from the aggregate");
        }

        topology.Single(x => x.Id == 4).Should().BeEquivalentTo(untouchedBefore.Single(x => x.Id == 4));

        // The restored row takes further events as usual.
        await Append(store, 1, TestData.Fixture.Create<RoadSegmentGeometryWasModified>() with
        {
            RoadSegmentId = new RoadSegmentId(1),
            Geometry = BuildRoadSegmentGeometry(0, 0, 20, 0),
            StartNodeId = null,
            EndNodeId = null
        });
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

    // On a connection of its own: a session's connection runs in the session's transaction, which is never committed here.
    private static async Task Execute(IDocumentStore store, string sql)
    {
        await using var connection = store.Storage.Database.CreateConnection();
        await connection.OpenAsync();
        await connection.ExecuteAsync(sql);
    }

    private static async Task<IReadOnlyList<TopologyRoadSegment>> QueryTopology(IDocumentStore store)
    {
        await using var session = store.QuerySession();
        return (await session.Connection!.QueryAsync<TopologyRoadSegment>(
                $"SELECT id AS Id, ST_AsText(geometry) AS Wkt, start_node_id AS StartNodeId, end_node_id AS EndNodeId, is_v2 AS IsV2 FROM {RoadNetworkTopologyProjection.RoadSegmentsTableName}"))
            .ToList();
    }

    private static string ReadDataFixMigration()
    {
        var assembly = typeof(RoadNetworkTopologyProjection).Assembly;
        var resourceName = assembly.GetManifestResourceNames().Single(x => x.EndsWith(DataFixMigration));

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private static async Task<RoadNetworkIds> GetUnderlyingIds(IServiceProvider sp)
    {
        var store = sp.GetRequiredService<IDocumentStore>();
        var repository = sp.GetRequiredService<IRoadNetworkRepository>();

        await using var session = store.LightweightSession();
        return await repository.GetUnderlyingIds(session, BuildRoadSegmentGeometry(5, -5, 5, 5).Value);
    }

    private sealed record TopologyRoadSegment(int Id, string Wkt, int? StartNodeId, int? EndNodeId, bool IsV2);
}
