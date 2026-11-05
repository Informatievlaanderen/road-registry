namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RunOnce;

using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Dapper;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Npgsql;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using Xunit.Abstractions;

/// <summary>
/// MartenMigrationProjection.RemoveRoadNode and ModifyRoadNode loaded the road node aggregate, applied an event to it
/// and never handed it back to the session. Their road segment counterparts do call session.Store, which is why only
/// road nodes drifted: the snapshot documents those two wrote are behind their own streams. On tst that was 206.552
/// road nodes still saying isRemoved: false after a V1 RoadNodeRemoved took them out, and 57 sitting on the point a
/// RoadNodeModified moved them off.
///
/// It is not only a stale flag. Since 1a813da62 the road network topology is looked up in these documents instead of
/// in the networktopology_* projection, so RoadNetworkRepository.GetUnderlyingIds* reads every one of those road
/// nodes as still existing.
///
/// This rewrites the documents the way the projection would have: it applies the same event to the aggregate and
/// stores it. Nothing is appended to any stream - the streams were always right, only the snapshots were not - so
/// no projection sees anything and this can run without a deploy.
///
/// WHICH ROAD NODES. Not "has a removal event somewhere in its stream": a road node can be removed and then added,
/// migrated or have its removal corrected again. The last event in the stream that moves existence either way says
/// where it stands now, and the same for the last one that sets the point. SQL only preselects on that; every
/// candidate is judged again on its own aggregate before anything is written.
///
/// This WRITES when <c>dryRun</c> is false. It is idempotent: a second run finds nothing, because it selects on the
/// difference it removes. Run it on STG before PRD, and afterwards let RoadRegistry.Pbs.Compare.Console confirm -
/// its "volgens de events verwijderd maar hun aggregate-document staat niet op isRemoved" note should be gone.
/// </summary>
public class RepairRoadNodeSnapshots
{
    private const int BatchSize = 500;

    // Marten's snake_case aliases, kept with the Existences list in RoadRegistry.Pbs.Compare.Console - whose
    // --eventtypes prints what the store actually holds, which is how both lists are checked against reality.
    private const string RemovedV1 = "road_node_removed";
    private const string RemovedV2 = "road_node_was_removed";
    private const string RemovedBecauseOfMigration = "road_node_was_removed_because_of_migration";
    private const string ModifiedV1 = "road_node_modified";

    private static readonly string[] Removals = [RemovedV1, RemovedV2, RemovedBecauseOfMigration];

    private static readonly string[] ExistenceEvents =
    [
        "imported_road_node",
        "road_node_added",
        "road_node_was_added",
        "road_node_was_migrated",
        "road_node_removal_was_corrected_because_of_migration",
        RemovedV1,
        RemovedV2,
        RemovedBecauseOfMigration
    ];

    // Everything RoadNode.Apply lets set the point. A removal correction does not: it only clears IsRemoved.
    private static readonly string[] GeometryEvents =
    [
        "imported_road_node",
        "road_node_added",
        "road_node_was_added",
        "road_node_was_migrated",
        "road_node_was_modified",
        ModifiedV1
    ];

    private readonly ITestOutputHelper _outputHelper;

    public RepairRoadNodeSnapshots(ITestOutputHelper outputHelper)
    {
        _outputHelper = outputHelper;
    }

    [Fact(Skip = "Run once, against an environment")]
    public async Task Run()
    {
        // Only reports what it would rewrite; set to false to write.
        var dryRun = true;

        var sp = BuildServiceProvider();
        var store = sp.GetRequiredService<IDocumentStore>();

        var removals = await FindUnmarkedRemovals(store);
        var geometries = await FindStaleGeometryCandidates(store);
        _outputHelper.WriteLine($"Unmarked removals: {removals.Count}");
        _outputHelper.WriteLine($"Stale geometry candidates: {geometries.Count}");
        _outputHelper.WriteLine(string.Empty);

        var repairedRemovals = await RepairRemovals(store, removals, dryRun);
        var repairedGeometries = await RepairGeometries(store, geometries, dryRun);

        _outputHelper.WriteLine(string.Empty);
        _outputHelper.WriteLine($"{(dryRun ? "Would repair" : "Repaired")}: {repairedRemovals} removals, {repairedGeometries} geometries");
    }

    private async Task<int> RepairRemovals(IDocumentStore store, IReadOnlyList<UnmarkedRemoval> removals, bool dryRun)
    {
        var repaired = 0;

        foreach (var batch in removals.Chunk(BatchSize))
        {
            await using var session = store.LightweightSession();

            // One round trip per batch rather than per road node: there are a few hundred thousand of these.
            var roadNodes = (await session.LoadManyAsync<RoadRegistry.RoadNode.RoadNode>(
                    batch.Select(x => StreamKeyFactory.Create(typeof(RoadRegistry.RoadNode.RoadNode), new RoadNodeId(x.RoadNodeId)))))
                .ToDictionary(x => x.RoadNodeId);

            foreach (var removal in batch)
            {
                var roadNodeId = new RoadNodeId(removal.RoadNodeId);
                if (!roadNodes.TryGetValue(roadNodeId, out var roadNode))
                {
                    throw new InvalidOperationException($"Road node {roadNodeId} has no document");
                }

                // Judged again on the aggregate: SQL read the generated column, this reads what it was generated from.
                if (roadNode.IsRemoved)
                {
                    continue;
                }

                // The same event the projection applied, so the document ends up saying what it should have said.
                if (removal.Type == RemovedBecauseOfMigration)
                {
                    roadNode.Apply(new RoadRegistry.RoadNode.Events.V2.RoadNodeWasRemovedBecauseOfMigration
                    {
                        RoadNodeId = roadNodeId,
                        // Required on the event, ignored by Apply, which only sets the two removal flags. The
                        // aggregate's own point is the one thing here that is certainly not a new claim.
                        Geometry = roadNode.Geometry,
                        Provenance = new ProvenanceData(Provenance())
                    });
                }
                else
                {
                    roadNode.Apply(new RoadRegistry.RoadNode.Events.V2.RoadNodeWasRemoved
                    {
                        RoadNodeId = roadNodeId,
                        Provenance = new ProvenanceData(Provenance())
                    });
                }

                session.Store(roadNode);
                repaired++;
            }

            if (!dryRun)
            {
                await session.SaveChangesAsync();
            }
        }

        return repaired;
    }

    private async Task<int> RepairGeometries(IDocumentStore store, IReadOnlyList<int> candidates, bool dryRun)
    {
        var repaired = 0;

        foreach (var batch in candidates.Chunk(BatchSize))
        {
            await using var session = store.LightweightSession();

            foreach (var id in batch)
            {
                var roadNodeId = new RoadNodeId(id);
                var streamKey = StreamKeyFactory.Create(typeof(RoadRegistry.RoadNode.RoadNode), roadNodeId);

                var events = await session.Events.FetchStreamAsync(streamKey);
                var lastModified = events
                    .Select(x => x.Data)
                    .OfType<RoadRegistry.RoadNode.Events.V1.RoadNodeModified>()
                    .LastOrDefault();
                if (lastModified is null)
                {
                    continue;
                }

                var roadNode = await session.LoadAsync(roadNodeId)
                               ?? throw new InvalidOperationException($"Road node {roadNodeId} has no document");

                // The same translation ModifyRoadNode does. Doing it here rather than in SQL is the point of this
                // runner: the document holds the point DotSpatial transformed to Lambert 2008, and ST_Transform does
                // not reproduce that to the centimetre these differences are measured in.
                var geometry = lastModified.Geometry.EnsureLambert08();
                if (roadNode.Geometry.WKT == geometry.WKT && roadNode.Geometry.SRID == geometry.SRID)
                {
                    continue;
                }

                _outputHelper.WriteLine($"{roadNodeId}: {roadNode.Geometry.WKT} -> {geometry.WKT}");

                roadNode.Apply(new RoadRegistry.RoadNode.Events.V2.RoadNodeWasModified
                {
                    RoadNodeId = roadNodeId,
                    Geometry = geometry,
                    Provenance = new ProvenanceData(Provenance())
                });

                session.Store(roadNode);
                repaired++;
            }

            if (!dryRun)
            {
                await session.SaveChangesAsync();
            }
        }

        return repaired;
    }

    private static Provenance Provenance()
        => new RoadRegistryProvenanceData(
            Modification.Update,
            reason: "Repair road node snapshots the marten migration left behind").ToProvenance();

    /// <summary>
    /// Road nodes whose stream ends on a removal while their document does not say so. The generated is_removed
    /// column is as current as the document it is generated from, so it is what the document's own answer is read
    /// from here.
    /// </summary>
    private static async Task<IReadOnlyList<UnmarkedRemoval>> FindUnmarkedRemovals(IDocumentStore store)
    {
        await using var session = store.QuerySession();

        var rows = await session.Connection!.QueryAsync<UnmarkedRemoval>($@"
SELECT doc.road_node_id AS {nameof(UnmarkedRemoval.RoadNodeId)}, last_existence.type AS {nameof(UnmarkedRemoval.Type)}
FROM {RoadNetworkTopologyTables.RoadNodes} doc
JOIN (
    SELECT DISTINCT ON (e.stream_id) e.stream_id, e.type
    FROM eventstore.mt_events e
    WHERE e.type = ANY(@existenceEvents)
    ORDER BY e.stream_id, e.version DESC
) last_existence ON last_existence.stream_id = doc.id
WHERE last_existence.type = ANY(@removals)
  AND NOT doc.is_removed
ORDER BY doc.road_node_id",
            new { existenceEvents = ExistenceEvents, removals = Removals });

        return rows.AsList();
    }

    /// <summary>
    /// Road nodes whose last event that sets the point is the V1 RoadNodeModified that ModifyRoadNode never stored,
    /// and whose stream does not end on a removal. Only a preselection: whether the point on the document actually
    /// differs is decided against the event itself, after the same transform the projection applies.
    /// </summary>
    /// <remarks>
    /// Existence is read from the stream here, not from the document's is_removed - that flag is the other half of
    /// what this runner repairs, so a road node about to be marked removed would otherwise have its point rewritten
    /// first. The point of a removed road node is nobody's business: the extract leaves it out and the compare tool
    /// does not look at it.
    /// </remarks>
    private static async Task<IReadOnlyList<int>> FindStaleGeometryCandidates(IDocumentStore store)
    {
        await using var session = store.QuerySession();

        var ids = await session.Connection!.QueryAsync<int>($@"
SELECT doc.road_node_id
FROM {RoadNetworkTopologyTables.RoadNodes} doc
JOIN (
    SELECT DISTINCT ON (e.stream_id) e.stream_id, e.type
    FROM eventstore.mt_events e
    WHERE e.type = ANY(@geometryEvents)
    ORDER BY e.stream_id, e.version DESC
) last_geometry ON last_geometry.stream_id = doc.id
JOIN (
    SELECT DISTINCT ON (e.stream_id) e.stream_id, e.type
    FROM eventstore.mt_events e
    WHERE e.type = ANY(@existenceEvents)
    ORDER BY e.stream_id, e.version DESC
) last_existence ON last_existence.stream_id = doc.id
WHERE last_geometry.type = @modifiedV1
  AND NOT last_existence.type = ANY(@removals)
ORDER BY doc.road_node_id",
            new
            {
                geometryEvents = GeometryEvents,
                existenceEvents = ExistenceEvents,
                modifiedV1 = ModifiedV1,
                removals = Removals
            });

        return ids.AsList();
    }

    private sealed class UnmarkedRemoval
    {
        public int RoadNodeId { get; init; }
        public string Type { get; init; } = string.Empty;
    }

    private static IConfiguration BuildConfiguration()
        => new ConfigurationBuilder()
            .UseDefaultConfiguration(new HostingEnvironment())
            .Build();

    private IServiceProvider BuildServiceProvider()
    {
        var configuration = BuildConfiguration();

        _outputHelper.WriteLine("Target:");
        _outputHelper.WriteLine($"  Marten (aggregates): {DescribePostgres(configuration.GetConnectionString(WellKnownConnectionNames.Marten))}");
        _outputHelper.WriteLine(string.Empty);

        var services = new ServiceCollection()
            .AddSingleton(configuration)
            .AddLogging();

        services.AddMartenRoad(options => options
            .AddRoadAggregatesSnapshots()
            .ConfigureExtractDocuments());

        return services.BuildServiceProvider().CreateScope().ServiceProvider;
    }

    private static string DescribePostgres(string? connectionString)
    {
        if (connectionString is null)
        {
            return "<not set>";
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        return $"{builder.Host}:{builder.Port}/{builder.Database}";
    }
}
