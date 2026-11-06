namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests.RunOnce;

using Dapper;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting.Internal;
using Npgsql;
using RoadRegistry.Extracts.Projections.Setup;
using RoadRegistry.Hosts;
using RoadRegistry.Hosts.Infrastructure.Extensions;
using RoadRegistry.Infrastructure;
using RoadRegistry.Infrastructure.MartenDb.Setup;
using RoadRegistry.Infrastructure.MartenDb.Store;
using Xunit.Abstractions;

/// <summary>
/// MartenMigrationProjection.RemoveOutlinedRoadSegment marked the aggregate removed on every V1
/// OutlinedRoadSegmentRemoved, and that event does not mean the road segment is gone.
///
/// The old system kept outlined and measured road segments in two SqlStreamStore streams. A road segment whose
/// geometry stopped being outlined moved from the one to the other: its outlined variant was removed while the road
/// segment itself carried on as measured, under the same id. Marten has a single stream per road segment, so that
/// move arrived as a removal of a road segment that is still very much there. A real removal comes in as
/// RoadSegmentRemoved. All 891 of these events in production are that move - every one preceded by a
/// RoadSegmentModified carrying convertedFromOutlined and the measured draw method - and the read, extract, WMS/WFS
/// and PBS projections all ignore the event outright, which is what the aggregate now does too.
///
/// WHICH ROAD SEGMENTS. Not "has an outlined removal somewhere in its stream": a road segment converted to measured
/// can be removed for real afterwards, and then its document is right to say so. What this looks for is a document
/// that says isRemoved while the last removal of any kind in its stream is the outlined one - that removed state
/// rests on nothing but the defect.
///
/// It also prints the draw method each road segment carried going into its outlined removal. That is a check on the
/// story rather than a decision: every one of them should read as measured, and a road segment that was still
/// outlined is worth looking at before writing anything, even though the repaired behaviour would not have removed
/// it either.
///
/// This WRITES when <c>dryRun</c> is false, and it writes the document JSON directly rather than applying an event:
/// the road segment aggregate has no event that undoes a removal, where its road node counterpart does
/// (RoadNodeRemovalWasCorrectedBecauseOfMigration). Nothing is appended to any stream - the streams were always
/// right, only the snapshots were not - so no projection sees anything and this can run without a deploy of its own.
/// It is idempotent: a second run finds nothing, because it selects on the difference it removes.
///
/// Run it on STG before PRD, and only after the RemoveOutlinedRoadSegment fix is deployed, or the migration will keep
/// producing what this cleans up.
/// </summary>
public class RepairRoadSegmentSnapshots
{
    private const int BatchSize = 500;

    // Marten's snake_case aliases, kept with the lists in RoadRegistry.Pbs.Compare.Console - whose --eventtypes
    // prints what the store actually holds, which is how these are checked against reality.
    private const string OutlinedRemovedV1 = "outlined_road_segment_removed";
    private const string RemovedV1 = "road_segment_removed";
    private const string RemovedV2 = "road_segment_was_removed";
    private const string RemovedBecauseOfMigration = "road_segment_was_removed_because_of_migration";

    private static readonly string[] OtherRemovals = [RemovedV1, RemovedV2, RemovedBecauseOfMigration];

    // The V1 events that carry a geometry draw method. RoadSegmentAttributesModified and RoadSegmentGeometryModified
    // do not, so a road segment's draw method is whatever the last one of these said.
    private static readonly string[] DrawMethodEvents = ["imported_road_segment", "road_segment_added", "road_segment_modified"];

    private readonly ITestOutputHelper _outputHelper;

    public RepairRoadSegmentSnapshots(ITestOutputHelper outputHelper)
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

        var candidates = await FindWronglyRemoved(store);
        _outputHelper.WriteLine($"Removed documents whose removal rests on an outlined removal: {candidates.Count}");
        _outputHelper.WriteLine(string.Empty);

        ReportDrawMethods(candidates);

        var repaired = await ClearRemovalFlag(store, candidates.Select(x => x.StreamKey).ToList(), dryRun);

        _outputHelper.WriteLine(string.Empty);
        _outputHelper.WriteLine($"{(dryRun ? "Would repair" : "Repaired")}: {repaired} road segments");
    }

    /// <summary>
    /// The draw method each road segment carried going into its outlined removal, grouped. A sanity check on the
    /// story this runner rests on, not a decision it makes: anything other than a measured draw method here wants
    /// looking at before writing.
    /// </summary>
    private void ReportDrawMethods(IReadOnlyList<WronglyRemoved> candidates)
    {
        foreach (var group in candidates.GroupBy(x => x.GeometryDrawMethod).OrderByDescending(x => x.Count()))
        {
            _outputHelper.WriteLine($"Draw method going into the removal - {group.Key}: {group.Count()}");

            // Every id, not a sample: a dry run is only worth running if it says exactly which documents the real
            // run would rewrite.
            _outputHelper.WriteLine($"  {string.Join(", ", group.Select(x => x.RoadSegmentId))}");
        }
    }

    /// <summary>
    /// Clears isRemoved on the document. Straight at the JSON, because the aggregate has no event that undoes a
    /// removal; where the generated is_removed column exists it follows the document, so it needs no touching of its
    /// own. Keyed on the document id - the stream key, and the primary key of the table. The WHERE repeats the
    /// removed condition, so a document that stopped being removed between the two statements is left alone.
    /// </summary>
    private async Task<int> ClearRemovalFlag(IDocumentStore store, IReadOnlyList<string> streamKeys, bool dryRun)
    {
        if (dryRun || streamKeys.Count == 0)
        {
            return streamKeys.Count;
        }

        var repaired = 0;

        foreach (var batch in streamKeys.Chunk(BatchSize))
        {
            await using var session = store.LightweightSession();

            repaired += await session.Connection!.ExecuteAsync($@"
UPDATE {RoadNetworkTopologyTables.RoadSegments}
SET data = jsonb_set(data, '{{isRemoved}}', 'false'::jsonb)
WHERE id = ANY(@streamKeys)
  AND coalesce((data ->> 'isRemoved')::boolean, false)",
                new { streamKeys = batch });
        }

        return repaired;
    }

    /// <summary>
    /// Road segments whose document says isRemoved while the last removal in their stream is the outlined one, so
    /// nothing but the defect put them there. A road segment removed for real after its conversion has a later
    /// RoadSegmentRemoved (or a V2 removal), and is left out by that comparison.
    /// </summary>
    /// <remarks>
    /// Read from the document JSON rather than from the road_segment_id and is_removed columns generated off it.
    /// Those columns arrive with the 20261103120000_add_topology_columns_to_aggregate_documents migration, which an
    /// environment still on the previous release does not have - and an environment in exactly that state is where
    /// this is most needed, because a command handler loading the aggregate reads isRemoved straight out of this
    /// JSON and turns it into "road segment does not exist", migration or no migration.
    /// </remarks>
    private static async Task<IReadOnlyList<WronglyRemoved>> FindWronglyRemoved(IDocumentStore store)
    {
        await using var session = store.QuerySession();

        var rows = await session.Connection!.QueryAsync<WronglyRemoved>($@"
WITH outlined AS (
    SELECT e.stream_id, max(e.version) AS version
    FROM eventstore.mt_events e
    WHERE e.type = @outlinedRemoved
    GROUP BY e.stream_id
),
other AS (
    SELECT e.stream_id, max(e.version) AS version
    FROM eventstore.mt_events e
    WHERE e.type = ANY(@otherRemovals)
      AND e.stream_id IN (SELECT stream_id FROM outlined)
    GROUP BY e.stream_id
)
SELECT doc.id AS {nameof(WronglyRemoved.StreamKey)},
       (doc.data ->> 'roadSegmentId')::int AS {nameof(WronglyRemoved.RoadSegmentId)},
       coalesce(preceding.data ->> 'geometryDrawMethod', '<none>') AS {nameof(WronglyRemoved.GeometryDrawMethod)}
FROM outlined
JOIN {RoadNetworkTopologyTables.RoadSegments} doc ON doc.id = outlined.stream_id
LEFT JOIN other ON other.stream_id = outlined.stream_id
LEFT JOIN LATERAL (
    SELECT m.data
    FROM eventstore.mt_events m
    WHERE m.stream_id = outlined.stream_id
      AND m.type = ANY(@drawMethodEvents)
      AND m.version < outlined.version
    ORDER BY m.version DESC
    LIMIT 1
) preceding ON true
WHERE coalesce((doc.data ->> 'isRemoved')::boolean, false)
  AND (other.version IS NULL OR other.version < outlined.version)
ORDER BY 2",
            new
            {
                outlinedRemoved = OutlinedRemovedV1,
                otherRemovals = OtherRemovals,
                drawMethodEvents = DrawMethodEvents
            });

        return rows.AsList();
    }

    private sealed class WronglyRemoved
    {
        public string StreamKey { get; init; } = string.Empty;
        public int RoadSegmentId { get; init; }
        public string GeometryDrawMethod { get; init; } = string.Empty;
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
